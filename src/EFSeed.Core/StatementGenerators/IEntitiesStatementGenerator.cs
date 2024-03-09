namespace EFSeed.Core.StatementGenerators;

public interface IEntitiesStatementGenerator
{
    string Generate(List<dynamic> entities);
}
