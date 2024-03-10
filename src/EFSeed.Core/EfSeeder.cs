using System.Text;
using EFSeed.Core.StatementGenerators;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core;

using Seed = IEnumerable<IEnumerable<dynamic>>;

public class EfSeeder
{
    private readonly IEntitiesStatementGeneratorFactory _statementGeneratorFactory;

    public EfSeeder(IEntitiesStatementGeneratorFactory statementGeneratorFactory )
    {
        _statementGeneratorFactory = statementGeneratorFactory;
    }

    public string CreateSeedScript(DbContext dbContext, Seed seed)
    {
        if (dbContext == null)
        {
            throw new ArgumentNullException(nameof(dbContext));
        }
        if(seed == null)
        {
            throw new ArgumentNullException(nameof(seed));
        }
        var entitiesLists = seed.ToList();
        var script = new StringBuilder();
        var entitiesScriptGenerator = _statementGeneratorFactory.Create(dbContext);
        foreach (var enumerable in entitiesLists)
        {
            var entities = enumerable.ToList();
            if (entities.Count == 0)
            {
                continue;
            }
            var entitiesScript = entitiesScriptGenerator.Generate(entities);
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
