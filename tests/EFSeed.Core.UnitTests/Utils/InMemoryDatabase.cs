using EFSeed.Core.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.UnitTests.Utils;

public class InMemoryDatabase : IDatabase
{
    public DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EFSeedTestDbContext>()
            .UseInMemoryDatabase("EFSeedTest")
            .Options;
        var context = new EFSeedTestDbContext(options);

        return context;
    }
}
