using System.Reflection;
using EFSeed.Cli.Generate;
using EFSeed.Core;

namespace EFSeed.Cli.Loading;

public class DatabaseSeedLoader(ProjectTypesExtractor typesExtractor)
{
    public IEnumerable<IEnumerable<object>> Load(GenerateOptions options)
    {
        var definedTypes = typesExtractor.GetProjectTypes(options);
        var seedType = definedTypes.FirstOrDefault(t => t.GetInterfaces().Any(i => i.FullName == typeof(IDatabaseSeed).FullName));
        if (seedType == null)
        {
            throw new InvalidOperationException("No database seed was found.");
        }
        var seed = Activator.CreateInstance(seedType);
        if (seed == null)
        {
            throw new InvalidOperationException("Database seed could not be created.");
        }
        var seedMethod = seedType.GetMethod(nameof(IDatabaseSeed.Seed));
        if (seedMethod == null)
        {
            throw new InvalidOperationException("Seed method not found.");
        }
        var seedData = seedMethod.Invoke(seed, null);
        if (seedData == null)
        {
            throw new InvalidOperationException("Seed data could not be created.");
        }
        return (seedData as IEnumerable<IEnumerable<object>>)!;
    }
}
