namespace EFSeed.Core.Tests.Common;

public class CustomDatabaseSeed : IDatabaseSeed
{
    public IEnumerable<IEnumerable<object>> Seed()
    {
        return new List<IEnumerable<Country>>
        {
            new List<Country> { new() { Id = 1, Name = "USA" } },
        };
    }
}
