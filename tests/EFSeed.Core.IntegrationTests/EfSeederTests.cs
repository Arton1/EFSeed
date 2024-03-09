using EFSeed.Core.StatementGenerators;
using EFSeed.Core.Tests.Common;
using EFSeed.Core.Tests.Utils;

namespace EFSeed.Core.Tests;


public class EfSeederTests : IClassFixture<MssqlDatabase>
{
    private readonly IDatabase _database;

    public EfSeederTests(MssqlDatabase database)
    {
        _database = database;
    }


    [Fact]
    public void Should_Run_Database()
    {
        var seeder = new EfSeeder(new EntitiesInsertStatementGeneratorFactory());
        var seed = new List<List<dynamic>>();
        using var context = _database.CreateDbContext();
        var script = seeder.CreateSeedScript(context, seed);
        Assert.Equal("", script);
    }
}
