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
        script.Append("BEGIN TRANSACTION;\n\n");
        var hasData = false;
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
                hasData = true;
            }
        }
        if(!hasData)
        {
            return "";
        }
        script.Append("\n");
        script.Append("COMMIT;");
        return script.ToString();
    }
}
