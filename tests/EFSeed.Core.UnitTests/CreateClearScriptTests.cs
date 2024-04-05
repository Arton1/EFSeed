using EFSeed.Core.Tests.Common;
using EFSeed.Core.UnitTests.Utils;

namespace EFSeed.Core.UnitTests;

public class CreateClearScriptTests : IClassFixture<InMemoryDatabase>
{
    private IDatabase _database;

    public CreateClearScriptTests(InMemoryDatabase database)
    {
        _database = database;
    }

    [Fact]
    public void Should_Create_Clear_Script()
    {
        var dbContext = _database.CreateCleanDbContext();
        var efSeeder = new EfSeederBuilder()
            .WithDbContext(dbContext)
            .Build();

        var clearScript = efSeeder.CreateClearScript();

        Assert.NotNull(clearScript);
    }
}
