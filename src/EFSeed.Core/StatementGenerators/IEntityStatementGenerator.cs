namespace EFSeed.Core.StatementGenerators;

internal interface IEntityStatementGenerator
{
    string Generate(List<dynamic> entities);
}
