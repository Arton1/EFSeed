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
        using var context = _database.CreateCleanDbContext();
        var seeder = new EfSeederBuilder().WithDbContext(context).Build();
        Assert.Throws<ArgumentNullException>(() => seeder.CreateSeedScript(null));
    }

    [Fact]
    public void Should_Throw_When_List_In_Seed_Is_Not_Homogenous()
    {
        using var context = _database.CreateCleanDbContext();
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
            "SET IDENTITY_INSERT Country ON;\n" +
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, N'Atlantis')\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;"
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" } } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n" +
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, N'Atlantis'),\n" +
            "(2, N'Lythania')\n" +
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
           "SET IDENTITY_INSERT Country ON;\n" +
           "INSERT INTO Country (Id, Name)\n" +
           "VALUES\n" +
           "(1, N'Atlantis'),\n" +
           "(2, N'Lythania')\n" +
           "SET IDENTITY_INSERT Country OFF;\n\n" +
           "SET IDENTITY_INSERT Country ON;\n" +
           "INSERT INTO Country (Id, Name)\n" +
           "VALUES\n" +
           "(3, N'Zoytaria')\n" +
           "SET IDENTITY_INSERT Country OFF;\n\n" +
           "COMMIT;"
        ];
        yield return
        [
            new List<List<dynamic>>
            {
                new() { new Animal {
                    Id = 1,
                    Name = "Beabaul",
                    ClassId = AnimalClassType.Mammal,
                    Age = 3,
                    RowVer = [0x01, 0x02, 0x03] // Should ignore
                }}
            },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Animal ON;\n" +
            "INSERT INTO Animal (Id, Age, ClassId, Name)\n" +
            "VALUES\n" +
            "(1, 3, 10, N'Beabaul')\n" +
            "SET IDENTITY_INSERT Animal OFF;\n\n" +
            "COMMIT;"
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" } },
            new() { new Animal() { Id = 1, Name = "Beabaul", ClassId = AnimalClassType.Mammal, Age = 3} }
        }, "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n" +
            "INSERT INTO Country (Id, Name)\n" +
            "VALUES\n" +
            "(1, N'Atlantis')\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "SET IDENTITY_INSERT Animal ON;\n" +
            "INSERT INTO Animal (Id, Age, ClassId, Name)\n" +
            "VALUES\n" +
            "(1, 3, 10, N'Beabaul')\n" +
            "SET IDENTITY_INSERT Animal OFF;\n\n" +
            "COMMIT;"
        ];
        yield return [new List<List<dynamic>> {
                new() { new Country() { Id = 1, Name = "Atlantis" } },
                new() { new City() { Id = 1, Name = "Aetherwind", CountryId = 1} }
            }, "BEGIN TRANSACTION;\n\n" +
               "SET IDENTITY_INSERT Country ON;\n" +
               "INSERT INTO Country (Id, Name)\n" +
               "VALUES\n" +
               "(1, N'Atlantis')\n" +
               "SET IDENTITY_INSERT Country OFF;\n\n" +
               "SET IDENTITY_INSERT City ON;\n" +
               "INSERT INTO City (Id, CountryId, Name)\n" +
               "VALUES\n" +
               "(1, 1, N'Aetherwind')\n" +
               "SET IDENTITY_INSERT City OFF;\n\n" +
               "COMMIT;"
        ];
        // Id is not an identity column, but is a primary key
        yield return [new List<List<PhoneModel>>() { new () {
             new PhoneModel { Id = 1, Size = 16.5m, SerialNumber = "S0G5AL" }}
        }, "BEGIN TRANSACTION;\n\n" +
           "INSERT INTO PhoneModel (Id, SerialNumber, Size)\n" +
           "VALUES\n" +
           "(1, N'S0G5AL', 16.5)\n\n" +
           "COMMIT;"
        ];
    }

    [Theory]
    [MemberData(nameof(SeedAndExpectedInsertScript))]
    public void Should_Create_Valid_Insert_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected)
    {
        using var context = _database.CreateCleanDbContext();
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
            "SET IDENTITY_INSERT Country ON;\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, N'Atlantis')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;",
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" }, new Country() { Id = 2, Name = "Lythania" } } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, N'Atlantis'),\n" +
            "(2, N'Lythania')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n" +
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
            "SET IDENTITY_INSERT Country ON;\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, N'Atlantis'),\n" +
            "(2, N'Lythania')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(3, N'Zoytaria')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "COMMIT;",
        ];
        yield return [new List<List<dynamic>> {
            new() { new Country() { Id = 1, Name = "Atlantis" } },
            new() { new Animal() { Id = 1, Name = "Beabaul", ClassId = AnimalClassType.Mammal, Age = 3} } },
            "BEGIN TRANSACTION;\n\n" +
            "SET IDENTITY_INSERT Country ON;\n" +
            "MERGE INTO Country AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, N'Atlantis')\n" +
            ") AS SOURCE (Id, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Name)\n" +
            "VALUES (Source.Id, Source.Name);\n" +
            "SET IDENTITY_INSERT Country OFF;\n\n" +
            "SET IDENTITY_INSERT Animal ON;\n" +
            "MERGE INTO Animal AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, 3, 10, N'Beabaul')\n" +
            ") AS SOURCE (Id, Age, ClassId, Name)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.Age = Source.Age, Target.ClassId = Source.ClassId, Target.Name = Source.Name\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, Age, ClassId, Name)\n" +
            "VALUES (Source.Id, Source.Age, Source.ClassId, Source.Name);\n" +
            "SET IDENTITY_INSERT Animal OFF;\n\n" +
            "COMMIT;"
        ];
        yield return [new List<List<PhoneModel>>() {
            new List<PhoneModel>() { new PhoneModel { Id = 1, Size = 16.5m, SerialNumber = "S0G5AL" }} },
            "BEGIN TRANSACTION;\n\n" +
            "MERGE INTO PhoneModel AS TARGET\n" +
            "USING (VALUES\n" +
            "(1, N'S0G5AL', 16.5)\n" +
            ") AS SOURCE (Id, SerialNumber, Size)\n" +
            "ON Target.Id = Source.Id\n" +
            "WHEN MATCHED THEN\n" +
            "UPDATE SET Target.SerialNumber = Source.SerialNumber, Target.Size = Source.Size\n" +
            "WHEN NOT MATCHED THEN\n" +
            "INSERT (Id, SerialNumber, Size)\n" +
            "VALUES (Source.Id, Source.SerialNumber, Source.Size);\n\n" +
            "COMMIT;"
        ];
    }

    [Theory]
    [MemberData(nameof(SeedAndExpectedMergeScript))]
    public void Should_Create_Valid_Merge_Script(IEnumerable<IEnumerable<dynamic>> seed, string expected)
    {
        using var context = _database.CreateCleanDbContext();
        var seeder = new EfSeederBuilder()
            .WithDbContext(context)
            .WithMode(GenerationMode.Merge)
            .Build();
        var script = seeder.CreateSeedScript(seed);
        Assert.Equal(expected, script);
    }
}
