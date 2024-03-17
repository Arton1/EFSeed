using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators;

internal class EntityMergeStatementGenerator : IEntityStatementGenerator
{
    private readonly DbContext _dbContext;

    public EntityMergeStatementGenerator(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string Generate(List<dynamic> entities)
    {
        var type = entities[0].GetType() as Type;
        var entityModel = _dbContext.Model.FindEntityType(type!);
        if(entityModel == null)
        {
            throw new ArgumentException("Entity type not found in the context");
        }
        var schema = entityModel.GetSchema();
        var tableName = entityModel.GetTableName();
        var tableRef = schema == null ? tableName : $"{schema}.{tableName}";
        var columns = entityModel.GetProperties().Select(p => p.GetColumnName()).ToList();
        var script = new StringBuilder();
        script.Append($"SET IDENTITY_INSERT {tableRef} ON;\n\n");
        script.Append($"MERGE INTO {tableRef} AS TARGET\nUSING (VALUES\n");
        var valuesListGenerator = new SqlValuesListGenerator(entityModel);
        foreach (var entity in entities)
        {
            var valuesList = valuesListGenerator.Generate(entity);
            script.Append($"{valuesList},\n");
        }
        script.Remove(script.Length - 2, 2);
        script.Append("\n) AS SOURCE (");
        script.Append(string.Join(", ", columns));
        script.Append(")\nON Target.Id = Source.Id\nWHEN MATCHED THEN\nUPDATE SET ");
        foreach (var property in entityModel.GetProperties())
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
        script.Append($"\n\nSET IDENTITY_INSERT {tableRef} OFF;");
        return script.ToString();
    }

}
