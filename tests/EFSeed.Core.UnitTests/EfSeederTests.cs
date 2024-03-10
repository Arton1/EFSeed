using EFSeed.Core.StatementGenerators;
using EFSeed.Core.Tests.Common;
using EFSeed.Core.UnitTests.Utils;

namespace EFSeed.Core.UnitTests;

public class EfSeederTests : IClassFixture<InMemoryDatabase>
{
    private IDatabase _database;
    private static readonly IEntitiesStatementGeneratorFactory Insert = new EntitiesInsertStatementGeneratorFactory();
    private static readonly IEntitiesStatementGeneratorFactory Merge = new EntitiesMergeStatementGeneratorFactory();

    public EfSeederTests(InMemoryDatabase database)
    {
        _database = database;
    }

    [Fact]
    public void Should_Throw_When_Context_Is_Null()
    {
        var seeder = new EfSeeder(Insert);
        var seed = new List<List<dynamic>>();
        Assert.Throws<ArgumentNullException>(() => seeder.CreateSeedScript(null, seed));
    }

    [Fact]
    public void Should_Throw_When_Seed_Is_Null()
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder(Insert);
        Assert.Throws<ArgumentNullException>(() => seeder.CreateSeedScript(context, null));
    }

    [Fact]
    public void Should_Throw_When_List_In_Seed_Is_Not_Homogenous()
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder(Insert);
        var seed = new List<List<dynamic>>
        {
            new () { new Person {}, new Country { } },
        };
        Assert.Throws<ArgumentException>(() => seeder.CreateSeedScript(context, seed));
    }

    public static IEnumerable<object[]> SeedAndExpectedInsertScript()
    {
        yield return [new List<List<dynamic>> { new() { } }, ""];
        yield return
        [
            new List<List<dynamic>> { new() { new Country() { Id = 1, Name = "Atlantis" } } },
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, 'Atlantis')"
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" } } },
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, 'Atlantis'),\n" +
            "(2, 'Lythania')"];
        yield return [new List<List<dynamic>> {
            new()
            {
                new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" }
            },
            new() { new Country() { Id = 3, Name = "Zoytaria" } }
        }, "INSERT INTO Country (Id, Name)\n" +
           "VALUES\n" +
           "(1, 'Atlantis'),\n" +
           "(2, 'Lythania')\n" +
           "INSERT INTO Country (Id, Name)\n" +
           "VALUES\n" +
           "(3, 'Zoytaria')"];
        yield return [new List<List<dynamic>> {
                new() { new Country() { Id = 1, Name = "Atlantis" } },
                new() { new Animal() { Id = 1, Name = "Beabaul", Age = 3, Species = "Dog"} }
            }, "INSERT INTO Country (Id, Name)\n" +
               "VALUES\n" +
               "(1, 'Atlantis')\n" +
               "INSERT INTO Animal (Id, Age, Name, Species)\n" +
               "VALUES\n" +
               "(1, 3, 'Beabaul', 'Dog')"];
    }

    [Theory]
    [MemberData(nameof(SeedAndExpectedInsertScript))]
    public void Should_Create_Valid_Insert_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected)
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder(Insert);
        var script = seeder.CreateSeedScript(context, seed);
        Assert.Equal(expected, script);
    }

    public static IEnumerable<object[]> SeedAndExpectedMergeScript()
    {
        yield return [new List<List<dynamic>> { new() { } }, ""];
        yield return
        [
            new List<List<dynamic>> { new() { new Country() { Id = 1, Name = "Atlantis" } } },
            "MERGE INTO Country AS TARGET USING (VALUES(1, 'Atlantis')) AS SOURCE (Id, Name) ON Target.Id = Source.Id WHEN MATCHED THEN UPDATE SET Target.Id = Source.Id, Target.Name = Source.Name WHEN NOT MATCHED THEN INSERT (Id, Name) VALUES (Source.Id, Source.Name);",
        ];
    }

    [Theory]
    [MemberData(nameof(SeedAndExpectedMergeScript))]
    public void Should_Create_Valid_Merge_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected)
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder(Merge);
        var script = seeder.CreateSeedScript(context, seed);
        Assert.Equal(expected, script);
    }
}
