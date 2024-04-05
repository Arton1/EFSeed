using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core.StatementGenerators.Merge;

internal class EntityMergeStatementGenerator : EntityStatementGenerator
{

    public EntityMergeStatementGenerator(DbContext dbContext) : base(dbContext)
    {

    }

    protected override void GenerateScript(StringBuilder script, GenerationContext context)
    {
        script.Append($"MERGE INTO {context.TableRef} AS TARGET\nUSING (VALUES\n");
        var valuesListGenerator = new SqlValuesListGenerator(context.Properties);
        foreach (var entity in context.Entities)
        {
            var valuesList = valuesListGenerator.Generate(entity);
            script.Append($"{valuesList},\n");
        }
        script.Remove(script.Length - 2, 2);
        script.Append("\n) AS SOURCE (");
        var columns = context.Properties.Select(x => x.GetColumnName()).ToList();
        script.Append(string.Join(", ", columns));
        script.Append(")\nON Target.Id = Source.Id\nWHEN MATCHED THEN\nUPDATE SET ");
        foreach (var property in context.Properties)
        {
            if (property.IsPrimaryKey())
            {
                continue;
            }
            var columnName = property.GetColumnName();
            script.Append($"Target.{columnName} = Source.{columnName}, ");
        }
        script.Remove(script.Length - 2, 2);
        script.Append("\nWHEN NOT MATCHED THEN\nINSERT (");
        script.Append(string.Join(", ", columns));
        script.Append(")\nVALUES (");
        script.Append(string.Join(", ", columns.Select(c => $"Source.{c}")));
        script.Append(");");
    }
}
