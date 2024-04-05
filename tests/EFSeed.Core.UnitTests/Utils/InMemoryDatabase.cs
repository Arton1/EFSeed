using EFSeed.Core.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.UnitTests.Utils;

public class InMemoryDatabase : IDatabase
{
    public DbContext CreateCleanDbContext()
    {
        var options = new DbContextOptionsBuilder<EFSeedTestDbContext>()
            .UseInMemoryDatabase("EFSeedTest")
            .Options;
        var context = new EFSeedTestDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }
}
