using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EFSeed.Core.Tests.Common;

public class EFSeedSqlServerTestContextFactory : IDesignTimeDbContextFactory<EFSeedTestDbContext>
{
    public EFSeedTestDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EFSeedTestDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=EFSeedTest;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new EFSeedTestDbContext(optionsBuilder.Options);
    }
}
