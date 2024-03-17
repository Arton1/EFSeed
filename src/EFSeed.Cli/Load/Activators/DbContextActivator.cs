using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EFSeed.Cli.Load.Activators;

public class DbContextActivator
{
    public DbContext CreateInstance(List<TypeInfo> types)
    {
        var factoryType = types.FirstOrDefault(t => typeof(IDesignTimeDbContextFactory<DbContext>).IsAssignableFrom(t.AsType()));
        if (factoryType == null)
        {
            throw new InvalidOperationException("No design time db context factory was found.");
        }
        var factoryInstance = (IDesignTimeDbContextFactory<DbContext>)Activator.CreateInstance(factoryType)!;
        var dbContext = factoryInstance.CreateDbContext([]);
        if (dbContext == null)
        {
            throw new InvalidOperationException("Db context could not be created.");
        }
        return dbContext;
    }
}
