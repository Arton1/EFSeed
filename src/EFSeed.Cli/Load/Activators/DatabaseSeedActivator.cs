using System.Reflection;
using EFSeed.Core;

namespace EFSeed.Cli.Load.Activators;

public class DatabaseSeedActivator
{
    public IDatabaseSeed CreateInstance(List<TypeInfo> types)
    {
        var seedType = types.FirstOrDefault(t => typeof(IDatabaseSeed).IsAssignableFrom(t.AsType()));
        if (seedType == null)
        {
            throw new InvalidOperationException("No database seed was found.");
        }
        return (IDatabaseSeed)Activator.CreateInstance(seedType)!;
    }
}
