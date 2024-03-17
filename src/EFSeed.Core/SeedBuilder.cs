namespace EFSeed.Core;

// It's actually SeedDefinitionBuilder, but named like this for simplicity
public class SeedBuilder
{
    private readonly List<List<object>> _data = new ();

    internal SeedBuilder()
    {

    }

    public SeedBuilder Add<T>(IEnumerable<T> entities) where T : class
    {
        var list = entities.Cast<object>().ToList();
        _data.Add(list);
        return this;
    }

    // Hidden, so it can't be called from IDatabaseSeed
    internal SeedDefinition Build() => new(_data);
}
