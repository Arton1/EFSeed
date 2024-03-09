using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators;

public class EntitiesMergeStatementGeneratorFactory : IEntitiesStatementGeneratorFactory
{
    public IEntitiesStatementGenerator Create(DbContext dbContext)
    {
        return new EntitiesMergeStatementGenerator(dbContext);
    }
}
