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

    public static IEnumerable<object[]> SeedAndExpectedScript()
    {
        yield return [new List<List<dynamic>> { new() { } }, "", Insert];
        yield return
        [
            new List<List<dynamic>> { new() { new Country() { Id = 1, Name = "Atlantis" } } },
            "INSERT INTO Country (Id, Name) VALUES (1, 'Atlantis')",
            Insert
        ];
        yield return
        [
            new List<List<dynamic>> { new() { new Country() { Id = 1, Name = "Atlantis" } } },
            "MERGE INTO Country AS TARGET USING (VALUES(1, 'Atlantis')) AS SOURCE (Id, Name) ON Target.Id = Source.Id WHEN MATCHED THEN UPDATE SET Target.Id = Source.Id, Target.Name = Source.Name WHEN NOT MATCHED THEN INSERT (Id, Name) VALUES (Source.Id, Source.Name);",
            Merge
        ];
    }

    [Theory]
    [MemberData(nameof(SeedAndExpectedScript))]
    public void Should_Create_Valid_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected, IEntitiesStatementGeneratorFactory statementGeneratorFactory)
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeeder(statementGeneratorFactory);
        var script = seeder.CreateSeedScript(context, seed);
        Assert.Equal(expected, script);
    }
}
