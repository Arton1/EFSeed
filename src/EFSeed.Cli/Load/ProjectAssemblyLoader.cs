using System.Diagnostics;
using System.Reflection;

namespace EFSeed.Cli.Load;

public class ProjectAssemblyLoaderOptions
{
    public bool NoBuild { get; set; }
    public string? Path { get; set; }
}

public class ProjectAssemblyLoader
{
    public Assembly GetProjectAssembly(ProjectAssemblyLoaderOptions options)
    {
        var path = options.Path ?? Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(path);
        var csprojFile = path.EndsWith(".csproj") ? path : Directory.GetFiles(path, "*.csproj").FirstOrDefault();
        if (csprojFile == null)
        {
            throw new InvalidOperationException("No project file was found.");
        }
        if (!options.NoBuild)
        {
            BuildAssembly(".");
        }
        var assemblyFileName = Path.GetFileNameWithoutExtension(csprojFile) + ".dll";
        var assemblyFilePath = Directory
            .EnumerateFiles(".", assemblyFileName, SearchOption.AllDirectories)
            .FirstOrDefault();
        if (assemblyFilePath == null)
        {
            throw new InvalidOperationException("No assembly was found.");
        }
        var absoluteAssemblyFilePath = Path.GetFullPath(assemblyFilePath);
        var loadContext = new PluginLoadContext(absoluteAssemblyFilePath);
        return loadContext.LoadFromAssemblyPath(absoluteAssemblyFilePath);
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
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if(process.ExitCode != 0)
        {
            Console.Error.WriteLine(output);
            throw new InvalidOperationException("Build failed.");
        }
    }
}
