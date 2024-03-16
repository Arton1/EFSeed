using System.Reflection;
using System.Runtime.Loader;

namespace EFSeed.Cli.Loading;

// Explanation: https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
// Additional source: https://github.com/natemcmaster/DotNetCorePlugins/blob/v0.1.0/src/Plugins/Loader/ManagedLoadContext.cs
class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        try
        {
            var defaultAssembly = Default.LoadFromAssemblyName(assemblyName);
            if (defaultAssembly != null)
            {
                return null;
            }
        }
        catch
        {
            // ignored
        }
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return libraryPath != null ? LoadUnmanagedDllFromPath(libraryPath) : IntPtr.Zero;
    }
}
