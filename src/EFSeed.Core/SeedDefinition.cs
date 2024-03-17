namespace EFSeed.Core;

public class SeedDefinition
{
    public List<List<object>> Seed { get; }

    public SeedDefinition(List<List<object>> data)
    {
        Seed = data;
    }
}
