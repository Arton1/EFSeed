using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators;

public interface IEntitiesStatementGeneratorFactory
{
    IEntitiesStatementGenerator Create(DbContext dbContext);
}
