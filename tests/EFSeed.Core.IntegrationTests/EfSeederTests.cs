using EFSeed.Core.StatementGenerators;
using EFSeed.Core.Tests.Common;
using EFSeed.Core.Tests.Utils;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.Tests;


public class EfSeederTests : IClassFixture<MssqlDatabase>
{
    private readonly IDatabase _database;

    public EfSeederTests(MssqlDatabase database)
    {
        _database = database;
    }


    [Fact]
    public void Should_Execute_Insert_Script()
    {
        var seeder = new EfSeeder(new EntitiesInsertStatementGeneratorFactory());
        var seed = new List<List<Country>>
        {
            new() { new Country { Id = 1, Name = "Atlantis" }, new Country { Id = 2, Name = "Lythania" } }
        };
        using var context = _database.CreateDbContext();
        context.Database.EnsureCreated();
        var script = seeder.CreateSeedScript(context, seed);
        var output = context.Database.ExecuteSqlRaw(script);
        Assert.Equal(2, output);
    }

    [Fact]
    public void Should_Execute_Merge_Script()
    {
        var seeder = new EfSeeder(new EntitiesMergeStatementGeneratorFactory());
        var seed = new List<List<Country>>
        {
            new() { new Country { Id = 1, Name = "Atlantis" }, new Country { Id = 2, Name = "Lythania" } }
        };
        using var context = _database.CreateDbContext();
        context.Database.EnsureCreated();
        var script = seeder.CreateSeedScript(context, seed);
        var output = context.Database.ExecuteSqlRaw(script);
        Assert.Equal(2, output);
    }
}
