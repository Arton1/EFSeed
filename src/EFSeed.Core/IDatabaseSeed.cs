namespace EFSeed.Core;

public interface IDatabaseSeed
{
    public IEnumerable<IEnumerable<object>> Seed();
}
