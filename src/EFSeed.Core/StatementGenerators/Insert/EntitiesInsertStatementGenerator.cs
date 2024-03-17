using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.StatementGenerators.Insert;

internal class EntityInsertStatementGenerator : IEntityStatementGenerator
{
    private readonly DbContext _dbContext;

    public EntityInsertStatementGenerator(DbContext dbContext)
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
        var columns = entityModel.GetProperties().Select(p => p.GetColumnName());
        var script = new StringBuilder();
        script.Append($"SET IDENTITY_INSERT {tableRef} ON;\n\n");
        script.Append($"INSERT INTO {tableRef} (");
        script.Append(string.Join(", ", columns));
        script.Append(")\nVALUES\n");
        var valuesListGenerator = new SqlValuesListGenerator(entityModel);
        foreach (var entity in entities)
        {
            var entityType = entity.GetType() as Type;
            if (entityType != type)
            {
                throw new ArgumentException("All entities in seed must be of the same type");
            }
            string valuesList = valuesListGenerator.Generate(entity);
            script.Append(valuesList);
            script.Append(",\n");
        }
        script.Remove(script.Length - 2, 2);
        script.Append($"\n\nSET IDENTITY_INSERT {tableRef} OFF;");
        return script.ToString();
    }
}
