using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using EFSeed.Cli.Generate;
using Microsoft.Extensions.DependencyModel;

namespace EFSeed.Cli.Loading;

public class ProjectTypesExtractor
{
    private readonly Dictionary<GenerateOptions, List<TypeInfo>> _typeCache = new();

    public List<TypeInfo> GetProjectTypes(GenerateOptions options)
    {
        if (_typeCache.TryGetValue(options, out var types))
        {
            return types;
        }
        var projectTypes = GetProjectTypesImpl(options);
        _typeCache[options] = projectTypes;
        return projectTypes;
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

        var assembly = new CustomAssemblyLoadContextOld(assemblyFile).LoadFromAssemblyPath(assemblyFile);
        return GetLoadableDefinedTypes(assembly).ToList();
    }

    private IEnumerable<TypeInfo> GetLoadableDefinedTypes(Assembly assembly)
    {
        try
        {
            return assembly.DefinedTypes;
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo!);
        }
    }

    private void BuildAssembly(string csprojFile)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{csprojFile}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        process.WaitForExit();
    }
}

// This class is used to load the main assembly only, and not the dependencies
public class CustomAssemblyLoadContextOld : AssemblyLoadContext
{
    private readonly string mainAssemblyToLoadPath;

    public CustomAssemblyLoadContextOld(string mainAssemblyToLoadPath)
    {
        this.mainAssemblyToLoadPath = mainAssemblyToLoadPath;
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var path = Path.Combine(Path.GetDirectoryName(mainAssemblyToLoadPath), $"{assemblyName.Name}.dll");
        if (File.Exists(path))
        {
            return LoadFromAssemblyPath(path);
        }

        return Default.LoadFromAssemblyName(assemblyName);
    }
}


public class CustomAssemblyLoadContext : AssemblyLoadContext
{
    private readonly string mainAssemblyToLoadPath;
    private readonly DependencyContext dependencyContext;
    private readonly Dictionary<string, Assembly> loadedAssemblies = new();

    public CustomAssemblyLoadContext(string mainAssemblyToLoadPath)
    {
        this.mainAssemblyToLoadPath = mainAssemblyToLoadPath;

        var depsJsonFile = Path.Combine(Path.GetDirectoryName(mainAssemblyToLoadPath), Path.GetFileNameWithoutExtension(mainAssemblyToLoadPath) + ".deps.json");
        if (File.Exists(depsJsonFile))
        {
            var dependencyContextJson = File.ReadAllText(depsJsonFile);
            var dependencyContextReader = new DependencyContextJsonReader();
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dependencyContextJson));
            dependencyContext = dependencyContextReader.Read(memoryStream);
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (loadedAssemblies.TryGetValue(assemblyName.FullName, out var load))
        {
            return load;
        }
        var assembly = LoadAssembly(assemblyName);
        loadedAssemblies[assemblyName.FullName] = assembly;
        if (assembly != null)
        {
            // Load dependencies of the loaded assembly
            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                if (loadedAssemblies.ContainsKey(referencedAssembly.FullName))
                {
                    continue;
                }
                var loadedDependency = Load(referencedAssembly);
                if (loadedDependency != null)
                {
                    loadedAssemblies[referencedAssembly.FullName] = loadedDependency;
                }
            }
        }
        return assembly;
    }

    private Assembly LoadAssembly(AssemblyName assemblyName)
    {
        var library = dependencyContext.RuntimeLibraries.FirstOrDefault(lib => lib.Name == assemblyName.Name);
        if (library != null)
        {
            foreach (var assemblyPath in library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths))
            {
                var path = Path.Combine(Path.GetDirectoryName(mainAssemblyToLoadPath), assemblyPath);
                if (File.Exists(path) && !path.Contains("\\ref\\"))
                {
                    return LoadFromAssemblyPath(path);
                }
            }
        }

        // If the assembly was not found in the .deps.json file, try to load it from the NuGet package directory
        var nugetPackageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        var assemblyFilePaths = Directory.GetFiles(nugetPackageDirectory, assemblyName.Name + ".dll", SearchOption.AllDirectories).Where(path => !path.Contains("\\ref\\"));

        foreach (var assemblyFilePath in assemblyFilePaths)
        {
            var loadedAssemblyName = AssemblyName.GetAssemblyName(assemblyFilePath);
            if (loadedAssemblyName.Version == assemblyName.Version)
            {
                return LoadFromAssemblyPath(assemblyFilePath);
            }
        }
        return Default.LoadFromAssemblyName(assemblyName);
    }
}
