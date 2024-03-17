using EFSeed.Core.StatementGenerators;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core;

public class EfSeeder
{
    private readonly SeedScriptGenerator _seedScriptGenerator;
    private readonly DbContext _dbContext;

    internal EfSeeder(SeedScriptGenerator seedScriptGenerator, DbContext dbContext)
    {
        _seedScriptGenerator = seedScriptGenerator;
        _dbContext = dbContext;
    }

    public string CreateSeedScript(IEnumerable<IEnumerable<dynamic>> seed)
    {
        ArgumentNullException.ThrowIfNull(seed);
        return _seedScriptGenerator.Generate(seed);
    }
}
