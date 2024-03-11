namespace EFSeed.Core;

public interface IDatabaseSeed
{
    IEnumerable<IEnumerable<object>> Seed();
}
