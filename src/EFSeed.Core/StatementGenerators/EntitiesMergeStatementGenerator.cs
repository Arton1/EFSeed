﻿using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators;

public class EntitiesMergeStatementGenerator : IEntitiesStatementGenerator
{
    private readonly DbContext _dbContext;

    public EntitiesMergeStatementGenerator(DbContext dbContext)
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
        script.Append($"MERGE INTO {tableRef} AS TARGET USING (VALUES");
        var valuesListGenerator = new SqlValuesListGenerator(entityModel);
        foreach (var entity in entities)
        {
            var valuesList = valuesListGenerator.Generate(entity);
            script.Append($"{valuesList}, ");
        }
        script.Remove(script.Length - 2, 2);
        script.Append(") AS SOURCE (");
        script.Append(string.Join(", ", columns));
        script.Append(") ON Target.Id = Source.Id WHEN MATCHED THEN UPDATE SET ");
        foreach (var property in entityModel.GetProperties())
        {
            var columnName = property.GetColumnName();
            script.Append($"Target.{columnName} = Source.{columnName}, ");
        }
        script.Remove(script.Length - 2, 2);
        script.Append(" WHEN NOT MATCHED THEN INSERT (");
        script.Append(string.Join(", ", columns));
        script.Append(") VALUES (");
        script.Append(string.Join(", ", columns.Select(c => $"Source.{c}")));
        script.Append(");");
        return script.ToString();
    }

}