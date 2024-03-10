using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.Tests.Common;

public class EFSeedTestDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Person> People { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Animal> Animals { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public int CityId { get; set; }
    public City City { get; set; }
}

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CountryId { get; set; }
    public Country Country { get; set; }
    public List<Person> People { get; set; }
}

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<City> Cities { get; set; }
}

public class Animal
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Species { get; set; }
}
