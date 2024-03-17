using System.Reflection;
using EFSeed.Cli.Load.Activators;
using EFSeed.Core;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Cli.Load;

public class ProjectDependenciesLoader
{
    private readonly Assembly _projectAssembly;

    private Lazy<List<TypeInfo>> ProjectTypes => new(() => _projectAssembly.GetLoadableDefinedTypes().ToList());

    private ProjectDependenciesLoader(Assembly projectAssembly)
    {
        _projectAssembly = projectAssembly;
    }

    public static ProjectDependenciesLoader Create(ProjectAssemblyLoaderOptions options)
    {
        var projectAssemblyLoader = new ProjectAssemblyLoader();
        var assembly = projectAssemblyLoader.GetProjectAssembly(options);
        return new ProjectDependenciesLoader(assembly);
    }

    public DbContext CreateDbContext()
    {
        var dbContextActivator = new DbContextActivator();
        return dbContextActivator.CreateInstance(ProjectTypes.Value);
    }

    public IDatabaseSeed CreateDatabaseSeed()
    {
        var databaseSeedActivator = new DatabaseSeedActivator();
        return databaseSeedActivator.CreateInstance(ProjectTypes.Value);
    }
}
