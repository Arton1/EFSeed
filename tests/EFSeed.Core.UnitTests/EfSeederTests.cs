using EFSeed.Core.StatementGenerators;
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
        Assert.Throws<ArgumentNullException>(() => new EfSeederBuilder().WithDbContext(null).Build());
    }

    [Fact]
    public void Should_Throw_When_Seed_Is_Null()
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeederBuilder().WithDbContext(context).Build();
        Assert.Throws<ArgumentNullException>(() => seeder.CreateSeedScript(null));
    }

    [Fact]
    public void Should_Throw_When_List_In_Seed_Is_Not_Homogenous()
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeederBuilder().WithDbContext(context).Build();
        var seed = new List<List<dynamic>>
        {
            new () { new Person {}, new Country { } },
        };
        Assert.Throws<ArgumentException>(() => seeder.CreateSeedScript(seed));
    }

    public static IEnumerable<object[]> SeedAndExpectedInsertScript()
    {
        yield return [new List<List<dynamic>> { new() { } }, ""];
        yield return
        [
            new List<List<dynamic>> { new() { new Country() { Id = 1, Name = "Atlantis" } } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, 'Atlantis')\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;"
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" } } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, 'Atlantis'),\n" +
            "(2, 'Lythania')\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;"
        ];
        yield return [new List<List<dynamic>> {
            new()
            {
                new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" }
            },
            new() { new Country() { Id = 3, Name = "Zoytaria" } }
        }, "BEGIN TRANSACTION;\n\n" +
           "SET IDENTITY_INSERT Country ON;\n\n" +
           "INSERT INTO Country (Id, Name)\n" +
           "VALUES\n" +
           "(1, 'Atlantis'),\n" +
           "(2, 'Lythania')\n\n" +
           "SET IDENTITY_INSERT Country OFF;\n" +
           "SET IDENTITY_INSERT Country ON;\n\n" +
           "INSERT INTO Country (Id, Name)\n" +
           "VALUES\n" +
           "(3, 'Zoytaria')\n\n" +
           "SET IDENTITY_INSERT Country OFF;\n\n" +
           "COMMIT;"
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" } },
            new() { new Animal() { Id = 1, Name = "Beabaul", Age = 3, Species = "Dog"} }
        }, "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, 'Atlantis')\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n" +
            "SET IDENTITY_INSERT Animal ON;\n\n" +
            "INSERT INTO Animal (Id, Age, Name, Species)\n" +
            "VALUES\n" +
            "(1, 3, 'Beabaul', 'Dog')\n\n" +
            "SET IDENTITY_INSERT Animal OFF;\n\n" +
            "COMMIT;"
        ];
    }

    [Theory]
    [MemberData(nameof(SeedAndExpectedInsertScript))]
    public void Should_Create_Valid_Insert_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected)
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeederBuilder().WithDbContext(context).Build();
        var script = seeder.CreateSeedScript(seed);
        Assert.Equal(expected, script);
    }

    public static IEnumerable<object[]> SeedAndExpectedMergeScript()
    {
        yield return [new List<List<dynamic>> { new() { } }, ""];
        yield return
        [
            new List<List<dynamic>> { new() { new Country() { Id = 1, Name = "Atlantis" } } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, 'Atlantis')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;",
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" } } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, 'Atlantis'),\n" +
            "(2, 'Lythania')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;",
        ];
        yield return [new List<List<dynamic>> {
            new()
            {
                new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" }
            },
            new() { new Country() { Id = 3, Name = "Zoytaria" } } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, 'Atlantis'),\n" +
            "(2, 'Lythania')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(3, 'Zoytaria')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;",
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" } },
            new() { new Animal() { Id = 1, Name = "Beabaul", Age = 3, Species = "Dog"} } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, 'Atlantis')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n\n" +
            "SET IDENTITY_INSERT Country OFF;\n" +
            "SET IDENTITY_INSERT Animal ON;\n\n" +
            "MERGE INTO Animal AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, 3, 'Beabaul', 'Dog')\n" +
            ") AS SOURCE (Id, Age, Name, Species)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Age = Source.Age, Target.Name = Source.Name, Target.Species = Source.Species\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Age, Name, Species)\n" +
            "VALUES (Source.Id, Source.Age, Source.Name, Source.Species);\n\n" +
            "SET IDENTITY_INSERT Animal OFF;\n\n" +
            "COMMIT;"
        ];
    }

    [Theory]
    [MemberData(nameof(SeedAndExpectedMergeScript))]
    public void Should_Create_Valid_Merge_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected)
    {
        using var context = _database.CreateDbContext();
        var seeder = new EfSeederBuilder()
            .WithDbContext(context)
            .WithMode(GenerationMode.Merge)
            .Build();
        var script = seeder.CreateSeedScript(seed);
        Assert.Equal(expected, script);
    }
}
