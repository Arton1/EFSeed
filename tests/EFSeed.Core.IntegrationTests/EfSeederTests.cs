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
        using var context = _database.CreateDbContext();
        context.Database.EnsureCreated();
        var seeder = new EfSeederBuilder().WithDbContext(_database.CreateDbContext()).Build();
        var seed = new List<List<Country>>
        {
            new() { new Country { Id = 1, Name = "Atlantis" }, new Country { Id = 2, Name = "Lythania" } }
        };
        var script = seeder.CreateSeedScript(seed);
        var output = context.Database.ExecuteSqlRaw(script);
        Assert.Equal(2, output);
    }

    [Fact]
    public void Should_Execute_Merge_Script()
    {
        using var context = _database.CreateDbContext();
        context.Database.EnsureCreated();
        var seeder = new EfSeederBuilder().WithDbContext(_database.CreateDbContext()).Build();
        var seed = new List<List<Country>>
        {
            new() { new Country { Id = 1, Name = "Atlantis" }, new Country { Id = 2, Name = "Lythania" } }
        };
        var script = seeder.CreateSeedScript(seed);
        var output = context.Database.ExecuteSqlRaw(script);
        Assert.Equal(2, output);
    }
}
