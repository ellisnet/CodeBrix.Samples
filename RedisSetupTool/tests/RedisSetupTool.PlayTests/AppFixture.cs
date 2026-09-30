using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using RedisSetupTool.DockerManagement;
using RedisSetupTool.DockerManagement.Instances;
using RedisSetupTool.DockerManagement.Models;
using RedisSetupTool.DockerManagement.Topologies;
using RedisSetupTool.Services;
using RedisSetupTool.ViewModels;
using RedisSetupTool.Views;

namespace RedisSetupTool.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private readonly IDockerManager _docker = DispatchProxy.Create<IDockerManager, DockerFixture>();
    private string _previousAutomation;
    public DockerFixture Docker => (DockerFixture)_docker;
    public MainViewModel Model => (MainViewModel)View.DataContext;

    protected override void Prepare()
    {
        _previousAutomation = Environment.GetEnvironmentVariable(StartupAutomation.ScriptVariable);
        Environment.SetEnvironmentVariable(StartupAutomation.ScriptVariable, null);
    }

    protected override Microsoft.UI.Xaml.Application CreateApplication() => new App(services =>
    {
        services.AddSingleton(_docker);
        services.AddSingleton<IHostPortAllocator, FixturePorts>();
    });

    protected override Task BeforeResetAsync()
    {
        Docker.Reachable = true;
        Docker.UnexpectedCalls.Clear();
        return Task.CompletedTask;
    }

    protected override async Task AfterResetAsync()
    {
        await Application.WaitForAsync(() => Model.IsBusy, busy => !busy);
        await Application.EvaluateAsync(() => Model.PauseAutoRefresh());
    }

    protected override void Cleanup() =>
        Environment.SetEnvironmentVariable(StartupAutomation.ScriptVariable, _previousAutomation);
}

// A strict, in-memory daemon boundary. No method reaches Docker or runs a process.
// DispatchProxy is part of .NET; unsupported operations fail instead of silently succeeding.
public class DockerFixture : DispatchProxy
{
    public bool Reachable { get; set; } = true;
    public List<string> UnexpectedCalls { get; } = new();

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
            new ContainerInfo { Id = "fixture-running", ShortId = "running", Name = "cache-primary",
                Image = "redis:8-alpine", State = "running", Status = "Up 5 minutes", IsRunning = true },
            new ContainerInfo { Id = "fixture-stopped", ShortId = "stopped", Name = "worker-stopped",
                Image = "alpine:3", State = "exited", Status = "Exited (0)", IsRunning = false }),
        "ListManagedContainersAsync" => List<ContainerInfo>(),
        "ListImagesAsync" => List(
            new ImageInfo { Id = "redis-image", ShortId = "redis", DisplayName = "redis:8-alpine", SizeBytes = 50_000_000, RepoTags = new[] { "redis:8-alpine" } },
            new ImageInfo { Id = "alpine-image", ShortId = "alpine", DisplayName = "alpine:3", SizeBytes = 8_000_000, RepoTags = new[] { "alpine:3" } }),
        "ListNetworksAsync" => List(new NetworkInfo { Id = "fixture-network", Name = "fixture-bridge", Driver = "bridge", Scope = "local" }),
        "ListVolumesAsync" => List(new VolumeInfo { Name = "fixture-data", Driver = "local", SizeBytes = 1024, RefCount = 1 }),
        "GetDiskUsageAsync" => Task.FromResult(new DaemonDiskUsage { TotalSizeBytes = 60_000_000 }),
        "AdviseAllContainersAsync" => List<AdvisorFindingInfo>(),
        "StreamEventsAsync" => EmptyEvents(),
        "Dispose" => null,
        _ => Unexpected(method.Name),
    };

    private object Unexpected(string name)
    {
        UnexpectedCalls.Add(name);
        throw new InvalidOperationException("Docker fixture does not implement " + name);
    }
    private static Task<IReadOnlyList<T>> List<T>(params T[] items) => Task.FromResult<IReadOnlyList<T>>(items);
    private static async IAsyncEnumerable<DaemonEvent> EmptyEvents()
    {
        await Task.CompletedTask;
        yield break;
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
