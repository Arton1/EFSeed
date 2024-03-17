using EFSeed.Core.Clearing;
using EFSeed.Core.StatementGenerators;

namespace EFSeed.Core;

public class EfSeeder
{
    private readonly SeedScriptGenerator _seedScriptGenerator;
    private readonly ClearScriptGenerator _clearScriptGenerator;

    internal EfSeeder(SeedScriptGenerator seedScriptGenerator, ClearScriptGenerator clearScriptGenerator)
    {
        _seedScriptGenerator = seedScriptGenerator;
        _clearScriptGenerator = clearScriptGenerator;
    }

    public string CreateSeedScript(IEnumerable<IEnumerable<dynamic>> seed)
    {
        ArgumentNullException.ThrowIfNull(seed);
        return _seedScriptGenerator.Generate(seed);
    }

    public async Task ClearDatabase()
    {
        _clearScriptGenerator.Generate();
    }
}
