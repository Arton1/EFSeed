using EFSeed.Cli.Generate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EFSeed.Cli.Loading;

public class DbContextLoader(ProjectTypesExtractor typesExtractor)
{
    public DbContext Load(GenerateOptions options)
    {
        var definedTypes = typesExtractor.GetProjectTypes(options);
        var factoryType = definedTypes.FirstOrDefault(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition().FullName == typeof(IDesignTimeDbContextFactory<>).FullName));
        if (factoryType == null)
        {
            throw new InvalidOperationException("No design time db context factory was found.");
        }
        var factoryInstance = Activator.CreateInstance(factoryType);
        if (factoryInstance == null)
        {
            throw new InvalidOperationException("Db context factory could not be created.");
        }
        var createDbContextMethod = factoryType.GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>.CreateDbContext));
        if (createDbContextMethod == null)
        {
            throw new InvalidOperationException("CreateDbContext method not found.");
        }
        var dbContext = createDbContextMethod.Invoke(factoryInstance, [new string[] {}]) as DbContext;
        if (dbContext == null)
        {
            throw new InvalidOperationException("Db context could not be created.");
        }
        return dbContext;
    }
}
