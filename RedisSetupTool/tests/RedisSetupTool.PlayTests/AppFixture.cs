using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using RedisSetupTool.DockerManagement;
using RedisSetupTool.DockerManagement.Exec;
using RedisSetupTool.DockerManagement.Instances;
using RedisSetupTool.DockerManagement.Models;
using RedisSetupTool.DockerManagement.Topologies;
using RedisSetupTool.RedisManagement;
using RedisSetupTool.RedisManagement.Results;
using RedisSetupTool.Services;
using RedisSetupTool.ViewModels;
using RedisSetupTool.Views;

namespace RedisSetupTool.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private readonly IDockerManager _docker = DispatchProxy.Create<IDockerManager, DockerFixture>();
    private readonly IRedisTopologyService _topologies = DispatchProxy.Create<IRedisTopologyService, TopologyFixture>();
    private readonly IRedisProbe _probe = DispatchProxy.Create<IRedisProbe, ProbeFixture>();
    private string _previousAutomation;
    public DockerFixture Docker => (DockerFixture)_docker;
    public TopologyFixture Topologies => (TopologyFixture)_topologies;
    public ProbeFixture Probe => (ProbeFixture)_probe;
    public MainViewModel Model => (MainViewModel)View.DataContext;

    protected override void Prepare()
    {
        _previousAutomation = Environment.GetEnvironmentVariable(StartupAutomation.ScriptVariable);
        Environment.SetEnvironmentVariable(StartupAutomation.ScriptVariable, null);
    }

    protected override Microsoft.UI.Xaml.Application CreateApplication() => new App(services =>
    {
        services.AddSingleton(_docker);
        services.AddSingleton(_topologies);
        // Verify on an instance card must never open a real Redis connection.
        services.AddSingleton(_probe);
        // RedisTopologyService takes the concrete manager, bypassing IDockerManager.
        // Fail at resolution if any service accidentally reaches the real daemon.
        services.AddSingleton<DockerManager>(_ => throw new InvalidOperationException(
            "PlayTests must not construct a real DockerManager."));
        services.AddSingleton<IHostPortAllocator, FixturePorts>();
    });

    protected override Task BeforeResetAsync()
    {
        // The previous page's console pumps and streams end here, so no gate outlives its test.
        Docker.Reset();
        Topologies.Reset();
        Probe.UnexpectedCalls.Clear();
        return Task.CompletedTask;
    }

    protected override async Task AfterResetAsync()
    {
        await Application.WaitForAsync(() => Model.IsBusy, busy => !busy);
        await Application.EvaluateAsync(() => Model.PauseAutoRefresh());
        if (Topologies.UnexpectedCalls.Count != 0)
            throw new InvalidOperationException("Unexpected topology fixture calls: " +
                string.Join(", ", Topologies.UnexpectedCalls));
    }

    protected override void Cleanup()
    {
        Docker.Reset();
        Topologies.Reset();
        Environment.SetEnvironmentVariable(StartupAutomation.ScriptVariable, _previousAutomation);
    }
}

// A strict, in-memory daemon boundary. No method reaches Docker or runs a process.
// DispatchProxy is part of .NET; unsupported operations fail instead of silently succeeding.
public class DockerFixture : DispatchProxy
{
    public const string RunningId = "fixture-running";
    public const string StoppedId = "fixture-stopped";
    private readonly object _gate = new();
    private readonly List<string> _calls = new();
    private readonly Dictionary<string, bool> _running = new();
    private readonly List<ImageInfo> _images = new();
    private readonly List<NetworkInfo> _networks = new();
    private readonly List<VolumeInfo> _volumes = new();
    private readonly List<FixtureExecSession> _sessions = new();
    private Channel<DaemonEvent> _events = Channel.CreateUnbounded<DaemonEvent>();
    private Channel<ContainerStatsSample> _stats = Channel.CreateUnbounded<ContainerStatsSample>();

    public DockerFixture() => Reset();

    public bool Reachable { get; set; } = true;
    // Non-null makes the next image pull fail with this exception.
    public Exception PullFailure { get; set; }
    public List<string> UnexpectedCalls { get; } = new();
    // Null makes the shell probe report that the image has no usable shell.
    public string ShellPath { get; set; }
    public string[] Calls { get { lock (_gate) return _calls.ToArray(); } }
    public FixtureExecSession[] Sessions { get { lock (_gate) return _sessions.ToArray(); } }

    public bool Called(string call) => Calls.Contains(call);
    public void EmitEvent(DaemonEvent daemonEvent) => _events.Writer.TryWrite(daemonEvent);
    public void EmitStats(ContainerStatsSample sample) => _stats.Writer.TryWrite(sample);

    public void Reset()
    {
        lock (_gate)
        {
            foreach (var session in _sessions) session.End();
            _sessions.Clear();
            _calls.Clear();
        }
        _events.Writer.TryComplete();
        _stats.Writer.TryComplete();
        _events = Channel.CreateUnbounded<DaemonEvent>();
        _stats = Channel.CreateUnbounded<ContainerStatsSample>();
        Reachable = true;
        PullFailure = null;
        ShellPath = "/bin/sh";
        UnexpectedCalls.Clear();
        _running.Clear();
        _running[RunningId] = true;
        _running[StoppedId] = false;
        _images.Clear();
        _images.Add(Image("redis-image", "redis:8-alpine", 50_000_000));
        _images.Add(Image("alpine-image", "alpine:3", 8_000_000));
        _networks.Clear();
        _networks.Add(new NetworkInfo { Id = "fixture-network", Name = "fixture-bridge", Driver = "bridge", Scope = "local" });
        _volumes.Clear();
        _volumes.Add(new VolumeInfo { Name = "fixture-data", Driver = "local", SizeBytes = 1024, RefCount = 1 });
    }

    protected override object Invoke(MethodInfo method, object[] args) => method.Name switch
    {
        "get_Endpoint" => "fixture://docker",
        "get_AdvisorRuleIds" => Array.Empty<string>(),
        "GetDaemonInfoAsync" => Task.FromResult(new DaemonInfo
        {
            IsReachable = Reachable, ServerVersion = "29.0-fixture", ApiVersion = "1.52",
            Endpoint = "fixture://docker", OperatingSystem = "PlayTest fixture", OsType = "linux",
            Architecture = "x86_64", CpuCount = 8, TotalMemoryBytes = 16L * 1024 * 1024 * 1024,
            ContainerCount = 2, ContainersRunning = 1, ContainersStopped = 1, ImageCount = 2,
        }),
        "ListContainersAsync" => List(
            Container(RunningId, "running", "cache-primary", "redis:8-alpine"),
            Container(StoppedId, "stopped", "worker-stopped", "alpine:3")),
        "ListManagedContainersAsync" => List<ContainerInfo>(),
        "ListImagesAsync" => List(_images.ToArray()),
        "ListNetworksAsync" => List(_networks.ToArray()),
        "ListVolumesAsync" => List(_volumes.ToArray()),
        "GetDiskUsageAsync" => Task.FromResult(new DaemonDiskUsage { TotalSizeBytes = 60_000_000 }),
        "AdviseAllContainersAsync" => List<AdvisorFindingInfo>(),
        "StreamEventsAsync" => Read(_events, (CancellationToken)args[0]),
        "InspectContainerAsync" => Task.FromResult(Inspect((string)args[0])),
        "StartContainerAsync" => Lifecycle(method, args, true),
        "StopContainerAsync" => Lifecycle(method, args, false),
        "RestartContainerAsync" => Lifecycle(method, args, true),
        "KillContainerAsync" => Lifecycle(method, args, false),
        "RemoveContainerAsync" => Record(method, args[0], args[1]),
        "PruneContainersAsync" => Record(method),
        "GetLogsAsync" => Logs(method, args),
        "StreamStatsAsync" => Read(_stats, (CancellationToken)args[1]),
        "DiagnoseAsync" => Task.FromResult(new DiagnosticsReport
        {
            ContainerId = (string)args[0], Summary = "Fixture diagnostics: nothing throttled.",
            Health = new HealthInfo { HasHealthcheck = false, Interpretation = "No healthcheck is defined." },
        }),
        "AdviseContainerAsync" => List<AdvisorFindingInfo>(),
        "PullImageAsync" => Pull(method, args),
        "InspectImageAsync" => Task.FromResult(new ImageDetail
        {
            Id = "sha256:" + args[0], DisplayName = (string)args[0], RepoTags = new[] { (string)args[0] },
            Architecture = "amd64", Os = "linux", LayerCount = 1, Cmd = new[] { "redis-server" },
        }),
        "GetImageHistoryAsync" => List(new ImageLayerInfo { CreatedBy = "ADD fixture-layer.tar /", SizeBytes = 4096 }),
        "TagImageAsync" => AddImage(method, args, (string)args[1]),
        "RemoveImageAsync" => RemoveImage(method, args),
        "PruneImagesAsync" => Record(method, args[0]),
        "ScanImageAsync" => Task.FromResult(new ImageScanReport
        {
            ImageReference = (string)args[0], Total = 1,
            CountBySeverity = new Dictionary<string, int> { ["HIGH"] = 1 },
            Vulnerabilities = new[]
            {
                new VulnerabilityInfo { Id = "CVE-2099-0001", Severity = "HIGH", PackageName = "fixture-lib", InstalledVersion = "1.0", FixedVersion = "1.1", HasFix = true },
            },
        }),
        "AnalyzeImageEfficiencyAsync" => Task.FromResult(new ImageEfficiencyReport
        {
            ImageReference = (string)args[0], EfficiencyScore = 0.987, WastedBytes = 2048, WastedPercent = 1.3,
            TotalSizeBytes = 50_000_000,
            Layers = new[] { new EfficiencyLayerInfo { Index = 0, SizeBytes = 4096, Command = "ADD fixture-layer.tar /" } },
        }),
        "LintDockerfileAsync" => Task.FromResult(new DockerfileLintReport
        {
            DockerfilePath = (string)args[0], Total = 1,
            Findings = new[] { new LintFindingInfo { Code = "DL3007", Level = "warning", Line = 1, Message = "Using latest is prone to errors." } },
        }),
        "CreateNetworkAsync" => Created(method, args, () => _networks.Add(new NetworkInfo { Id = "id-" + args[0], Name = (string)args[0], Driver = "bridge", Scope = "local" })),
        "RemoveNetworkAsync" => Removed(method, args, () => _networks.RemoveAll(n => n.Name == (string)args[0])),
        "PruneNetworksAsync" => Record(method),
        "CreateVolumeAsync" => Created(method, args, () => _volumes.Add(new VolumeInfo { Name = (string)args[0], Driver = "local", SizeBytes = 0, RefCount = 0 })),
        "RemoveVolumeAsync" => Removed(method, args, () => _volumes.RemoveAll(v => v.Name == (string)args[0])),
        "PruneVolumesAsync" => Record(method),
        "ProbeShellAsync" => Task.FromResult(ShellPath == null
            ? new ShellProbeResult { Found = false, Tried = new[] { "/bin/bash", "/bin/sh" }, Message = "Fixture image has no shell." }
            : new ShellProbeResult { Found = true, ShellPath = ShellPath, Tried = new[] { ShellPath } }),
        "OpenShellAsync" => Task.FromResult<IExecSession>(OpenShell((string)args[0], (ExecSessionOptions)args[1])),
        "Dispose" => null,
        _ => Unexpected(method.Name),
    };

    private ContainerInfo Container(string id, string shortId, string name, string image)
    {
        var running = _running[id];
        return new ContainerInfo
        {
            Id = id, ShortId = shortId, Name = name, Image = image, State = running ? "running" : "exited",
            Status = running ? "Up 5 minutes" : "Exited (0)", IsRunning = running,
        };
    }

    private ContainerDetail Inspect(string id) => new()
    {
        Id = id, Name = id == RunningId ? "cache-primary" : "worker-stopped", Image = "redis:8-alpine",
        StateStatus = _running[id] ? "running" : "exited", IsRunning = _running[id],
        Command = new[] { "redis-server", "--appendonly", "yes" }, WorkingDir = "/data", Hostname = "fixture-host",
        Env = new[] { "REDIS_VERSION=8.0-fixture" },
    };

    private Task Lifecycle(MethodInfo method, object[] args, bool running)
    {
        var id = (string)args[0];
        if (method.Name == "KillContainerAsync") Note(method.Name + " " + id + " " + args[1]);
        else Note(method.Name + " " + id);
        _running[id] = running;
        return Task.CompletedTask;
    }

    private Task<ContainerLogText> Logs(MethodInfo method, object[] args)
    {
        var tail = (int?)args[1];
        var timestamps = (bool)args[2];
        Note(method.Name + " " + args[0] + " tail=" + (tail?.ToString() ?? "all") + " timestamps=" + timestamps);
        return Task.FromResult(new ContainerLogText
        {
            Stdout = (timestamps ? "2099-01-01T00:00:00Z " : "") + "Ready to accept connections (tail "
                + (tail?.ToString() ?? "all") + ")\n",
        });
    }

    private Task Pull(MethodInfo method, object[] args)
    {
        if (PullFailure != null)
        {
            ((IProgress<string>)args[1])?.Report("fixture layer: Pulling fs layer");
            Note(method.Name + " " + (string)args[0]);
            return Task.FromException(PullFailure);
        }

        ((IProgress<string>)args[1])?.Report("fixture layer: Pull complete");
        return AddImage(method, args, (string)args[0]);
    }

    private Task AddImage(MethodInfo method, object[] args, string reference)
    {
        Note(method.Name + " " + string.Join(" ", args.OfType<string>()));
        _images.Add(Image("image-" + reference, reference, 1_000_000));
        return Task.CompletedTask;
    }

    private Task RemoveImage(MethodInfo method, object[] args)
    {
        Note(method.Name + " " + args[0]);
        _images.RemoveAll(i => i.DisplayName == (string)args[0]);
        return Task.CompletedTask;
    }

    private Task<string> Created(MethodInfo method, object[] args, Action change)
    {
        Note(method.Name + " " + args[0]);
        change();
        return Task.FromResult("id-" + args[0]);
    }

    private Task Removed(MethodInfo method, object[] args, Action change)
    {
        Note(method.Name + " " + args[0]);
        change();
        return Task.CompletedTask;
    }

    private Task Record(MethodInfo method, params object[] args)
    {
        Note(args.Length == 0 ? method.Name : method.Name + " " + string.Join(" ", args));
        return Task.CompletedTask;
    }

    private FixtureExecSession OpenShell(string id, ExecSessionOptions options)
    {
        var session = new FixtureExecSession(id, ShellPath, options.Columns, options.Rows);
        lock (_gate) _sessions.Add(session);
        Note("OpenShellAsync " + id);
        return session;
    }

    private void Note(string call)
    {
        lock (_gate) _calls.Add(call);
    }

    private object Unexpected(string name)
    {
        UnexpectedCalls.Add(name);
        throw new InvalidOperationException("Docker fixture does not implement " + name);
    }
    private static ImageInfo Image(string id, string name, long size) =>
        new() { Id = id, ShortId = id.Split('-')[0], DisplayName = name, SizeBytes = size, RepoTags = new[] { name } };
    private static Task<IReadOnlyList<T>> List<T>(params T[] items) => Task.FromResult<IReadOnlyList<T>>(items);
    // A scripted stream: each test writes the items it needs; leaving the page cancels the read.
    private static async IAsyncEnumerable<T> Read<T>(Channel<T> channel, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
            yield return item;
    }
}

// A scripted shell. Reads wait for test output or the end of the session, so the pump stays running.
public sealed class FixtureExecSession : IExecSession
{
    private readonly Channel<byte[]> _output = Channel.CreateUnbounded<byte[]>();
    private readonly List<string> _sent = new();

    public FixtureExecSession(string containerId, string shellPath, int columns, int rows)
    {
        ContainerId = containerId;
        ShellPath = shellPath;
        Columns = columns;
        Rows = rows;
    }

    public string ContainerId { get; }
    public string ExecId => "exec-" + ContainerId;
    public string ShellPath { get; }
    public bool IsTty => true;
    public bool UsesRawFraming => true;
    public bool CanCloseStandardInput => true;
    public bool IsRunning { get; private set; } = true;
    public bool Disposed { get; private set; }
    public int Columns { get; }
    public int Rows { get; }
    public long ExitCode { get; private set; }
    public string SentText { get { lock (_sent) return string.Concat(_sent); } }

    public void Write(string text) => _output.Writer.TryWrite(Encoding.UTF8.GetBytes(text));

    public void End(long exitCode = 0)
    {
        ExitCode = exitCode;
        _output.Writer.TryComplete();
    }

    public async Task<ExecReadResult> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (await _output.Reader.WaitToReadAsync(cancellationToken) && _output.Reader.TryRead(out var data))
        {
            data.CopyTo(buffer);
            return new ExecReadResult(ExecStreamKind.StandardOutput, data.Length);
        }
        IsRunning = false;
        return new ExecReadResult(ExecStreamKind.None, 0);
    }

    public Task SendAsync(string text, CancellationToken cancellationToken = default)
    {
        lock (_sent) _sent.Add(text);
        return Task.CompletedTask;
    }

    public Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default) =>
        SendAsync(Encoding.UTF8.GetString(data.Span), cancellationToken);

    public Task ResizeAsync(int rows, int columns, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CloseStandardInputAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<long> WaitForExitAsync(CancellationToken cancellationToken = default) => Task.FromResult(ExitCode);

    public void Dispose()
    {
        Disposed = true;
        End(ExitCode);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}

// The real catalog with in-memory instances. Creating, lifecycle actions and destroying
// only change this list; nothing is validated against or created on a daemon.
public class TopologyFixture : DispatchProxy
{
    private readonly List<string> _calls = new();
    public List<string> UnexpectedCalls { get; } = new();
    public List<TopologyInstance> Instances { get; } = new();
    // Create reports one progress step, then waits here until the test releases it.
    public TaskCompletionSource CreateGate { get; private set; }
    public Exception CreateFailure { get; set; }
    public string[] Calls { get { lock (_calls) return _calls.ToArray(); } }

    public bool Called(string call) => Calls.Contains(call);

    public void Reset()
    {
        CreateGate?.TrySetCanceled();
        CreateGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        CreateFailure = null;
        Instances.Clear();
        UnexpectedCalls.Clear();
        lock (_calls) _calls.Clear();
    }

    protected override object Invoke(MethodInfo method, object[] args) => method.Name switch
    {
        "get_Catalog" => TopologyCatalog.All,
        "Describe" => TopologyCatalog.Get((TopologyId)args[0]),
        "Validate" => Array.Empty<string>(),
        "PreviewPortsAsync" => Task.FromResult(new PortPlan { DataPorts = new[] { 16379 } }),
        "DiscoverAsync" => Task.FromResult<IReadOnlyList<TopologyInstance>>(Instances.ToArray()),
        "CreateAsync" => CreateAsync((TopologyRequest)args[0], (IProgress<TopologyProgress>)args[1], (CancellationToken)args[2]),
        "StartAsync" => Lifecycle(method, (string)args[0], true),
        "StopAsync" => Lifecycle(method, (string)args[0], false),
        "RestartAsync" => Lifecycle(method, (string)args[0], true),
        "DestroyAsync" => Destroy(method, (string)args[0]),
        _ => Unexpected(method.Name),
    };

    public static TopologyInstance Instance(string id, string name, bool running, string password = null)
    {
        var node = new TopologyNode
        {
            ContainerId = "container-" + id, ContainerName = name + "-node", Role = NodeRole.Primary,
            ContainerPort = 6379, HostPort = 16379, IsRunning = running, State = running ? "running" : "exited",
        };
        return new TopologyInstance
        {
            InstanceId = id, InstanceName = name, TopologyId = password == null ? TopologyId.A1 : TopologyId.A2,
            TopologyCode = password == null ? "A1" : "A2", Image = "redis:8-alpine",
            State = running ? InstanceState.Running : InstanceState.Stopped, NetworkName = name + "-net",
            VolumeNames = new[] { name + "-data" }, Nodes = new[] { node },
            Connection = new ConnectionInfo
            {
                Shape = ConnectionShape.Standalone, Password = password,
                Endpoints = new[] { new RedisEndpoint { Host = "127.0.0.1", Port = 16379, Role = NodeRole.Primary } },
                ConnectionString = "127.0.0.1:16379" + (password == null ? "" : ",password=" + password),
                CliCommand = "redis-cli -p 16379",
            },
        };
    }

    private async Task<TopologyInstance> CreateAsync(TopologyRequest request, IProgress<TopologyProgress> progress, CancellationToken cancellationToken)
    {
        Note("CreateAsync " + request.TopologyId + " " + request.InstanceName);
        progress?.Report(new TopologyProgress { Step = 1, TotalSteps = 2, Message = "Reserving fixture ports" });
        await CreateGate.Task.WaitAsync(cancellationToken);
        if (CreateFailure != null) throw CreateFailure;
        var instance = Instance("created-" + request.InstanceName, request.InstanceName, true);
        Instances.Add(instance);
        return instance;
    }

    private Task Lifecycle(MethodInfo method, string id, bool running)
    {
        Note(method.Name + " " + id);
        var index = Instances.FindIndex(i => i.InstanceId == id);
        if (index >= 0) Instances[index] = Instance(id, Instances[index].InstanceName, running, Instances[index].Connection.Password);
        return Task.CompletedTask;
    }

    private Task Destroy(MethodInfo method, string id)
    {
        Note(method.Name + " " + id);
        Instances.RemoveAll(i => i.InstanceId == id);
        return Task.CompletedTask;
    }

    private void Note(string call)
    {
        lock (_calls) _calls.Add(call);
    }

    private object Unexpected(string name)
    {
        UnexpectedCalls.Add(name);
        throw new InvalidOperationException("Topology fixture does not implement " + name);
    }
}

// Stands in for the real Redis client tier: Verify gets a scripted answer, everything else fails.
public class ProbeFixture : DispatchProxy
{
    public List<string> UnexpectedCalls { get; } = new();
    public RedisConnectionDescriptor LastVerified { get; private set; }

    protected override object Invoke(MethodInfo method, object[] args)
    {
        if (method.Name != "VerifyAsync")
        {
            UnexpectedCalls.Add(method.Name);
            throw new InvalidOperationException("Redis probe fixture does not implement " + method.Name);
        }
        LastVerified = (RedisConnectionDescriptor)args[0];
        return Task.FromResult(new RedisTopologyVerification
        {
            Succeeded = true, Summary = "Fixture verification passed.",
            Checks = new[] { new RedisVerificationCheck { Name = "PING", Passed = true, Detail = "PONG" } },
        });
    }
}

public sealed class FixturePorts : IHostPortAllocator
{
    public Task<PortPlan> PreviewAsync(TopologyDescriptor descriptor, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PortPlan { DataPorts = new[] { 16379 } });
    public Task<PortPlan> AllocateAsync(TopologyDescriptor descriptor, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("This UI fixture does not create Redis instances.");
    public void Release(PortPlan plan) { }
}
