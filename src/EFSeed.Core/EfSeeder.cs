using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core;

using Seed = IEnumerable<IEnumerable<dynamic>>;

public class EfSeeder
{
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
        if(entitiesLists.Count == 0)
        {
            return "";
        }
        var script = new StringBuilder();
        foreach (var enumerable in entitiesLists)
        {
            var entities = enumerable.ToList();
            if (entities.Count == 0)
            {
                continue;
            }
            var type = entities[0].GetType() as Type;
            var entityModel = dbContext.Model.FindEntityType(type!);
            if(entityModel == null)
            {
                throw new ArgumentException("Entity type not found in the context");
            }
            var entitiesScript = InitializeEntityTypeScript(entityModel, dbContext);
            var properties = entityModel.GetProperties();
            foreach (var entity in entities)
            {
                var entityType = entity.GetType() as Type;
                if (entityType != type)
                {
                    throw new ArgumentException("All entities in seed must be of the same type");
                }
                entitiesScript.Append("(");
                foreach (var property in properties)
                {
                    var value = entity.GetType().GetProperty(property.Name)?.GetValue(entity);
                    entitiesScript.Append($"{FormatValue(value)}, ");
                }
                entitiesScript.Remove(entitiesScript.Length - 2, 2);
                entitiesScript.Append("),");
            }
            entitiesScript.Remove(entitiesScript.Length - 1, 1);
            script.Append(entitiesScript);
        }
        return script.ToString();
    }

    private StringBuilder InitializeEntityTypeScript(IEntityType model, DbContext context)
    {
        var tableName = model.GetTableName();
        var columns = model.GetProperties().Select(p => p.GetColumnName());
        var script = new StringBuilder();
        script.Append($"INSERT INTO [{tableName}] (");
        script.Append(string.Join(", ", columns));
        script.Append(") VALUES ");
        return script;
    }

    private string FormatValue(object value) =>
        value switch
        {
            null => "NULL",
            string or DateTime => $"'{value}'",
            bool b => b ? "1" : "0",
            _ => value.ToString()!
        };
}
