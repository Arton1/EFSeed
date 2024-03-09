using EFSeed.Core.Tests.Common;
using EFSeed.Core.UnitTests.Utils;

namespace EFSeed.Core.UnitTests;

public class EfSeederTests : IClassFixture<InMemoryDatabase>
{
    private IDatabase _database;

    public EfSeederTests(InMemoryDatabase database)
    {
        _database = database;
    }

    [Fact]
    public void Should_Throw_When_Context_Is_Null()
    {
        var seeder = new EfSeeder();
        var seed = new List<List<dynamic>>();
        Assert.Throws<ArgumentNullException>(() => seeder.CreateSeedScript(null, seed));
    }

    [Fact]
    public void Should_Throw_When_Seed_Is_Null()
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder();
        Assert.Throws<ArgumentNullException>(() => seeder.CreateSeedScript(context, null));
    }

    [Fact]
    public void Should_Throw_When_List_In_Seed_Is_Not_Homogenous()
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder();
        var seed = new List<List<dynamic>>
        {
            new () { new Person {}, new Country { } },
        };
        Assert.Throws<ArgumentException>(() => seeder.CreateSeedScript(context, seed));
    }

    public static IEnumerable<object[]> SeedAndExpectedScript()
    {
        yield return [new List<List<dynamic>> { new() { } }, ""];
        yield return [new List<List<dynamic>> { new() { new Country() {Id = 1, Name = "Atlantis"} } },
            "INSERT INTO [Country] (Id, Name) VALUES (1, 'Atlantis')"];
    }


    [Theory]
    [MemberData(nameof(SeedAndExpectedScript))]
    public void Should_Create_Valid_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected)
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder();
        var script = seeder.CreateSeedScript(context, seed);
        Assert.Equal(expected, script);
    }
}
