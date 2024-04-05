using Bogus;

namespace EFSeed.Core.Tests.Common;

public class CustomDatabaseSeed : IDatabaseSeed
{

    public void Seed(SeedBuilder builder)
    {
        Faker.DefaultStrictMode = true;

        var countriesFaker = new Faker<Country>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.Name, f => f.Address.Country())
            .RuleFor(c => c.Cities, f => new List<City>());
        var countries = countriesFaker.Generate(5).ToList();
        builder.Add(countries);
        var citiesFaker = new Faker<City>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.Name, f => f.Address.City())
            .RuleFor(c => c.Country, f => f.PickRandom(countries))
            .RuleFor(c => c.CountryId, (f, c) => c.Country.Id)
            .RuleFor(c => c.People, f => new List<Person>());
        var cities = citiesFaker.Generate(20).ToList();
        builder.Add(cities);
        var phoneModelsFaker = new Faker<PhoneModel>()
            .RuleFor(p => p.Id, f => f.IndexFaker + 1)
            .RuleFor(p => p.SerialNumber, f => f.Random.AlphaNumeric(10))
            .RuleFor(p => p.Size, f => f.Random.Number(1, 10));
        var phoneModels = phoneModelsFaker.Generate(10).ToList();
        builder.Add(phoneModels);
        var peopleFaker = new Faker<Person>()
            .RuleFor(p => p.Id, f => f.IndexFaker + 1)
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Age, f => f.Random.Number(1, 100))
            .RuleFor(p => p.City, f => f.PickRandom(cities))
            .RuleFor(p => p.CityId, (f, p) => p.City.Id)
            .RuleFor(p => p.PhoneModel, f => f.PickRandom(phoneModels))
            .RuleFor(p => p.PhoneModelId, (f, p) => p.PhoneModel.Id);
        var people = peopleFaker.Generate(100).ToList();
        builder.Add(people);

        var animalsFaker = new Faker<Animal>()
            .RuleFor(a => a.Id, f => f.IndexFaker + 1)
            .RuleFor(a => a.Name, f => f.Random.Word())
            .RuleFor(a => a.Age, f => f.Random.Number(1, 10))
            .RuleFor(a => a.Class, f => null!)
            .RuleFor(a => a.ClassId, f => f.Random.Enum<AnimalClassType>())
            .RuleFor(a => a.RowVer, f => f.Random.Bytes(8));
        var animals = animalsFaker.Generate(10).ToList();
        builder.Add(animals);
    }
}
