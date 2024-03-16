using System.Diagnostics;
using System.Reflection;
using EFSeed.Cli.Generate;
using EFSeed.Core;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Cli.Loading;

public class ProjectTypesExtractor
{
    private static readonly Type[] SharedPluginTypes =
    [
        typeof(IDatabaseSeed),
        typeof(DbContext)
    ];

    public List<TypeInfo> GetProjectTypes(GenerateOptions options)
    {
        return GetProjectTypesImpl(options);
    }

    private List<TypeInfo> GetProjectTypesImpl(GenerateOptions options)
    {
        var projectDirectory = options.Project ?? Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(projectDirectory);
        var csprojFile = Directory.GetFiles(projectDirectory, "*.csproj").FirstOrDefault();
        if (csprojFile == null)
        {
            throw new InvalidOperationException("No project was found.");
        }
        if (!options.NoBuild)
        {
            BuildAssembly(csprojFile);
        }
        var assemblyFileName = Path.GetFileNameWithoutExtension(csprojFile) + ".dll";
        var assemblyFile = Directory
            .EnumerateFiles(projectDirectory, assemblyFileName, SearchOption.AllDirectories)
            .FirstOrDefault();
        if (assemblyFile == null)
        {
            throw new InvalidOperationException("No assembly was found.");
        }

        var assembly = LoadAssembly(assemblyFile);
        var types = assembly.GetLoadableDefinedTypes().ToList();
        return types;
    }

    private Assembly LoadAssembly(string path)
    {
        var loadContext = new PluginLoadContext(path);
        var assembly = loadContext.LoadFromAssemblyPath(path);
        return assembly;
    }

    private void BuildAssembly(string csprojFile)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                // AssemblyDependencyResolver requires CopyLocalLockFileAssemblies to be true, when executing dependency of project
                Arguments = $"build \"{csprojFile}\" --nologo --verbosity quiet -p:CopyLocalLockFileAssemblies=true",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        process.StandardOutput.ReadToEnd();
        process.WaitForExit();
    }
}
