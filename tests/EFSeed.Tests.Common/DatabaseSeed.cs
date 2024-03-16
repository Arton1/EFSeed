using Bogus;

namespace EFSeed.Core.Tests.Common;

public class CustomDatabaseSeed : IDatabaseSeed
{

    public void Seed(SeedBuilder builder)
    {
        var countriesFaker = new Faker<Country>()
            .RuleFor(c => c.Name, f => f.Address.Country());
        var countries = countriesFaker.Generate(5).ToList();
        builder.Add(countries);
    }
}
