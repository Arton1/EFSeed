namespace EFSeed.Core;

public static class IDatabaseSeedExtensions
{
    // Not part of contract
    public static SeedDefinition GenerateDefinition(this IDatabaseSeed seed)
    {
        var builder = new SeedBuilder();
        seed.Seed(builder);
        return builder.Build();
    }
}
