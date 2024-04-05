using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core.StatementGenerators.Merge;

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
        var includedProperties = entityModel
            .GetProperties()
            .Where(x => x.ValueGenerated != ValueGenerated.OnAddOrUpdate)
            .ToList();
        var isIdentityInsert = entityModel.HasIdentityInsert();
        var script = new StringBuilder();
        if (isIdentityInsert)
        {
            script.Append($"SET IDENTITY_INSERT {tableRef} ON;\n\n");
        }
        script.Append($"MERGE INTO {tableRef} AS TARGET\nUSING (VALUES\n");
        var valuesListGenerator = new SqlValuesListGenerator(includedProperties);
        foreach (var entity in entities)
        {
            var valuesList = valuesListGenerator.Generate(entity);
            script.Append($"{valuesList},\n");
        }
        script.Remove(script.Length - 2, 2);
        script.Append("\n) AS SOURCE (");
        var columns = includedProperties.Select(x => x.GetColumnName()).ToList();
        script.Append(string.Join(", ", columns));
        script.Append(")\nON Target.Id = Source.Id\nWHEN MATCHED THEN\nUPDATE SET ");
        foreach (var property in includedProperties)
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
        if (isIdentityInsert)
        {
            script.Append($"\n\nSET IDENTITY_INSERT {tableRef} OFF;");
        }
        return script.ToString();
    }

}
