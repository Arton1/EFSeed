using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyModel;

namespace EFSeed.Cli.Generate;

public class DbContextLoader
{
    public DbContext LoadDbContext(GenerateOptions options, string[] args)
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
        var assemblyFileName = Path.GetFileNameWithoutExtension(csprojFile) + ".dll";
        var assemblyFile = Directory
            .EnumerateFiles(projectDirectory, assemblyFileName, SearchOption.AllDirectories)
            .FirstOrDefault();
        if (assemblyFile == null)
        {
            throw new InvalidOperationException("No assembly was found.");
        }

        var assembly = new CustomAssemblyLoadContextOld(assemblyFile).LoadFromAssemblyPath(assemblyFile);
        var definedTypes = assembly.GetLoadableDefinedTypes().ToList();
        var factoryType = definedTypes.FirstOrDefault(t => typeof(IDesignTimeDbContextFactory<DbContext>).IsAssignableFrom(t));
        if (factoryType == null)
        {
            throw new InvalidOperationException("No design time db context factory was found.");
        }
        var toolDatabaseConfigInstance = Activator.CreateInstance(factoryType) as IDesignTimeDbContextFactory<DbContext>;
        var dbContext = toolDatabaseConfigInstance?.CreateDbContext(args);
        if (dbContext == null)
        {
            throw new InvalidOperationException("Db context could not be created.");
        }
        return dbContext;
    }
}

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

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var library = dependencyContext.RuntimeLibraries.FirstOrDefault(lib => lib.Name == assemblyName.Name);
        if (library != null)
        {
            foreach (var assemblyPath in library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths))
            {
                var path = Path.Combine(Path.GetDirectoryName(mainAssemblyToLoadPath), assemblyPath);
                if (File.Exists(path))
                {
                    return LoadFromAssemblyPath(path);
                }
            }
        }

        // If the assembly was not found in the .deps.json file, try to load it from the NuGet package directory
        var nugetPackageDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        var assemblyFilePath = Directory.GetFiles(nugetPackageDirectory, assemblyName.Name + ".dll", SearchOption.AllDirectories).FirstOrDefault();
        if (assemblyFilePath != null)
        {
            return LoadFromAssemblyPath(assemblyFilePath);
        }

        return Default.LoadFromAssemblyName(assemblyName);
    }
}
