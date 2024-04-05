using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators.Insert;

internal class EntityInsertStatementGenerator : EntityStatementGenerator
{

    public EntityInsertStatementGenerator(DbContext dbContext) : base(dbContext)
    {

    }

    protected override void GenerateScript(StringBuilder script, GenerationContext context)
    {
        script.Append($"INSERT INTO {context.TableRef} (");
        var columnNames = context.Properties.Select(prop => prop.GetColumnName()).ToList();
        script.Append(string.Join(", ", columnNames));
        script.Append(")\nVALUES\n");
        var valuesListGenerator = new SqlValuesListGenerator(context.Properties);
        foreach (var entity in context.Entities)
        {
            var valuesList = valuesListGenerator.Generate(entity);
            script.Append(valuesList);
            script.Append(",\n");
        }
        script.Remove(script.Length - 2, 2);
    }
}
