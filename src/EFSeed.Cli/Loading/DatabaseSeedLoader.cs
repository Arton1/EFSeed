using EFSeed.Cli.Generate;
using EFSeed.Core;

namespace EFSeed.Cli.Loading;

public class DatabaseSeedLoader(ProjectTypesExtractor typesExtractor)
{
    public List<List<object>> Load(GenerateOptions options)
    {
        var definedTypes = typesExtractor.GetProjectTypes(options);
        var seedType = definedTypes.FirstOrDefault(t => typeof(IDatabaseSeed).IsAssignableFrom(t.AsType()));
        if (seedType == null)
        {
            throw new InvalidOperationException("No database seed was found.");
        }
        var seed = (IDatabaseSeed)Activator.CreateInstance(seedType)!;
        var builder = new SeedBuilder();
        seed.Seed(builder);
        return builder.Build();
    }
}
