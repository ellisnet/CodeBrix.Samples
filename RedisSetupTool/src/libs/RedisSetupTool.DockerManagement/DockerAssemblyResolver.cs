using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading;

namespace RedisSetupTool.DockerManagement;

//The CodeBrix.Docker package reference carries PrivateAssets=all, which is what stops any downstream
//  project compiling against its types - the boundary this library exists to hold. That same setting
//  keeps the assembly out of every consumer's deps.json, so the default load context cannot find
//  CodeBrix.Docker.dll even though the build copies the file next to this assembly. This resolver
//  closes the gap once, inside the one library that owns the reference, so the heads,
//  RedisSetupTool.Core and the test projects need no plumbing of their own.
//A module initializer is the trigger, not a static constructor on DockerManager: the runtime has to
//  resolve CodeBrix.Docker while it prepares that constructor's body, which is earlier than the
//  constructor's first statement. The module initializer runs before any code in this assembly does,
//  so the hook is always in place by then.
internal static class DockerAssemblyResolver
{
    private const string DockerAssemblyName = "CodeBrix.Docker";

    private static int _registered;

    //CA2255 warns that module initializers belong in application code. This is the one case the
    //  rule cannot cover: the hook has to be installed before any code in THIS assembly runs, and
    //  only this assembly knows where its private copy of CodeBrix.Docker.dll sits.
#pragma warning disable CA2255
    [ModuleInitializer]
    internal static void EnsureRegistered()
    {
        if (Interlocked.Exchange(ref _registered, 1) == 0)
        {
            AssemblyLoadContext.Default.Resolving += ResolveDockerAssembly;
        }
    }
#pragma warning restore CA2255

    private static Assembly ResolveDockerAssembly(AssemblyLoadContext context, AssemblyName name)
    {
        if (!string.Equals(name?.Name, DockerAssemblyName, StringComparison.Ordinal))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(typeof(DockerAssemblyResolver).Assembly.Location);
        if (string.IsNullOrEmpty(directory))
        {
            return null;
        }

        var candidate = Path.Combine(directory, DockerAssemblyName + ".dll");
        return File.Exists(candidate) ? context.LoadFromAssemblyPath(candidate) : null;
    }
}
