using System.Text;

namespace EFSeed.Core.StatementGenerators;

internal class SeedScriptGenerator
{
    private readonly IEntityStatementGenerator _statementGenerator;

    public SeedScriptGenerator(IEntityStatementGenerator statementGenerator)
    {
        _statementGenerator = statementGenerator;
    }

    public string Generate(IEnumerable<IEnumerable<object>> seed)
    {
        var script = new StringBuilder();
        var entitiesLists = seed.ToList();
        foreach (var enumerable in entitiesLists)
        {
            var entities = enumerable.ToList();
            if (entities.Count == 0)
            {
                continue;
            }
            var entitiesScript = _statementGenerator.Generate(entities);
            if (entitiesScript.Length > 0)
            {
                script.Append(entitiesScript);
                script.Append("\n");
            }
        }
        if(script.Length == 0)
        {
            return "";
        }
        script.Remove(script.Length - 1, 1);
        return script.ToString();
    }
}
