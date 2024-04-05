using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.Tests.Common;

public class EFSeedTestDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Person> People { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<AnimalClass> AnimalClasses { get; set; }
    public DbSet<PhoneModel> PhoneModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This data is predefined and assumed to always be available
        modelBuilder.Entity<AnimalClass>()
            .HasData(
                typeof(AnimalClassType)
                    .GetEnumValues()
                    .Cast<AnimalClassType>()
                    .Select(x => new AnimalClass { Id = x, Name = x.ToString() }
                )
            );
        modelBuilder.Entity<AnimalClass>()
            .Property(x => x.Id)
            .HasConversion<int>();

        modelBuilder.Entity<Animal>()
            .Property(x => x.NameLength)
            .HasComputedColumnSql("LEN(Name)");

        modelBuilder.Entity<Animal>()
            .HasOne(x => x.Class)
            .WithMany()
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
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
    public long NameLength { get; }
    public int Age { get; set; }

    [Timestamp]
    public byte[] RowVer { get; set; }

    public AnimalClassType ClassId { get; set; }
    public AnimalClass Class { get; set; }
}

public enum AnimalClassType
{
    Mammal = 10,
    Bird = 20,
    Reptile = 30,
    Amphibian = 40,
    Fish = 50,
    Invertebrate = 60
}

public class AnimalClass
{

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public AnimalClassType Id { get; set; }
    public string Name { get; set; }
}

public class PhoneModel
{
    // Synchronized with other "source of truth" service by leveraging eventual consistency
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public string SerialNumber { get; set; }
    public decimal Size { get; set; }
}
