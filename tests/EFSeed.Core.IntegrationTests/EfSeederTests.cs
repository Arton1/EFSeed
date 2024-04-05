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

    [Theory]
    [InlineData(GenerationMode.Insert)]
    [InlineData(GenerationMode.Merge)]
    public void Should_Execute_Insert_Script(GenerationMode mode)
    {
        using var context = _database.CreateCleanDbContext();
        var seeder = new EfSeederBuilder()
            .WithMode(mode)
            .WithDbContext(context)
            .Build();
        var seed = new List<List<Country>>
        {
            new() { new Country { Id = 1, Name = "Atlantis" }, new Country { Id = 2, Name = "Lythania" } }
        };
        var script = seeder.CreateSeedScript(seed);
        var actual = context.Database.ExecuteSqlRaw(script);
        Assert.Equal(2, actual);
    }

    [Theory]
    [InlineData(GenerationMode.Insert)]
    [InlineData(GenerationMode.Merge)]
    public void Should_Execute_Seed_In_Database_Seed_Class(GenerationMode mode)
    {
        using var context = _database.CreateCleanDbContext();
        var seeder = new EfSeederBuilder()
            .WithMode(mode)
            .WithDbContext(context)
            .Build();
        var seed = new CustomDatabaseSeed().GenerateDefinition().Seed;

        var script = seeder.CreateSeedScript(seed);
        var actual = context.Database.ExecuteSqlRaw(script);

        var expected = seed.SelectMany(x => x).Count();
        Assert.Equal(expected, actual);
    }
}
