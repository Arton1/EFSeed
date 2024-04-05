using EFSeed.Core.StatementGenerators.Insert;
using EFSeed.Core.StatementGenerators.Merge;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators;

public enum GenerationMode
{
    Insert,
    Merge
}

internal class EntityStatementGeneratorFactory
{
    private readonly DbContext _dbContext;

    public EntityStatementGeneratorFactory(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEntityStatementGenerator Create(GenerationMode mode) =>
        mode switch
        {
            GenerationMode.Insert => new EntityInsertStatementGenerator(_dbContext),
            GenerationMode.Merge => new EntityMergeStatementGenerator(_dbContext),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
}
