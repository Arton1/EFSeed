using EFSeed.Core.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace EFSeed.Core.Tests.Utils;

public class MssqlDatabase : IDatabase, IAsyncLifetime
{
    private MsSqlContainer? _container;

    public async Task InitializeAsync()
    {
        var container = new MsSqlBuilder().Build();
        await container.StartAsync();
        var dbContext = CreateDbContext(container.GetConnectionString());
        await dbContext.Database.EnsureCreatedAsync();
        _container = container;
    }

    private DbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<EFSeedTestDbContext>()
            .UseSqlServer($"{connectionString};Persist Security Info=true")
            .Options;
        return new EFSeedTestDbContext(options);
    }

    public DbContext CreateCleanDbContext()
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }
        var dbContext = CreateDbContext(_container!.GetConnectionString());
        // TODO: Clean db tables
        return dbContext;
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}
