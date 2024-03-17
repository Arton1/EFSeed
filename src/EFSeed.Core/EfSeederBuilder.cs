using EFSeed.Core.Clearing;
using EFSeed.Core.StatementGenerators;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core;


public class EfSeederBuilder
{
    private DbContext _dbContext;
    private GenerationMode _mode = GenerationMode.Insert;
    private bool _isBuilt;

    // Dependency injection only
    public EfSeederBuilder()
    {

    }

    /// <summary>
    /// Sets the DbContext for the EfSeeder.
    /// </summary>
    /// <param name="dbContext">The DbContext to use.</param>
    /// <returns>The builder instance.</returns>
    public EfSeederBuilder WithDbContext(DbContext dbContext)
    {
        _dbContext = dbContext;
        return this;
    }


    /// <summary>
    /// Sets the GenerationMode for the EfSeeder.
    /// </summary>
    /// <param name="mode">The GenerationMode to use.</param>
    /// <returns>The builder instance.</returns>
    public EfSeederBuilder WithMode(GenerationMode mode)
    {
        _mode = mode;
        return this;
    }


    /// <summary>
    /// Builds the EfSeeder instance.
    /// </summary>
    /// <returns>The built EfSeeder instance.</returns>
    public EfSeeder Build()
    {
        if(_isBuilt)
        {
            throw new InvalidOperationException("EFSeederBuilder can only build one EfSeeder instance");
        }
        ArgumentNullException.ThrowIfNull(_dbContext);
        var statementGeneratorFactory = new EntityStatementGeneratorFactory(_dbContext);
        var statementGenerator = statementGeneratorFactory.Create(_mode);
        var seedScriptGenerator = new SeedScriptGenerator(statementGenerator);
        var clearScriptGenerator = new ClearScriptGenerator();
        var seeder = new EfSeeder(seedScriptGenerator, clearScriptGenerator);
        _isBuilt = true;
        return seeder;
    }
}
