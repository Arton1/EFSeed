using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators;

public class EntitiesInsertStatementGeneratorFactory : IEntitiesStatementGeneratorFactory
{
    public IEntitiesStatementGenerator Create(DbContext dbContext) => new EntitiesInsertStatementGenerator(dbContext);
}
