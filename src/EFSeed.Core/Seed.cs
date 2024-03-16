namespace EFSeed.Core;

public class SeedBuilder
{
    private readonly List<List<object>> _data = new ();

    public SeedBuilder Add<T>(IEnumerable<T> entities) where T : class
    {
        var list = entities.Cast<object>().ToList();
        _data.Add(list);
        return this;
    }

    public List<List<object>> Build()
    {
        return _data;
    }
}
