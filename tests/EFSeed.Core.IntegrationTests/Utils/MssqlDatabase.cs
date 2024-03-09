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
        _container = container;
    }

    public DbContext CreateDbContext()
    {
        if (_container is null)
        {
            throw new InvalidOperationException("Database is not initialized");
        }

        var connectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<EFSeedTestDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        var context = new EFSeedTestDbContext(options);

        return context;
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}
