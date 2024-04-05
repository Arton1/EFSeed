using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core.StatementGenerators;

public abstract class EntityStatementGenerator : IEntityStatementGenerator
{
    private readonly DbContext _dbContext;

    protected EntityStatementGenerator(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string Generate(List<object> entities)
    {
        var type = entities[0].GetType();
        var entityModel = _dbContext.Model.FindEntityType(type);
        if(entityModel == null)
        {
            throw new ArgumentException("Entity type not found in the context");
        }
        var schema = entityModel.GetSchema();
        var tableName = entityModel.GetTableName();
        var tableRef = schema == null ? tableName : $"{schema}.{tableName}";
        var includedProperties = entityModel.GetProperties()
            // Exclude auto-generated and computed columns
            .Where(prop => prop.ValueGenerated != ValueGenerated.OnAddOrUpdate && prop.GetComputedColumnSql() == null)
            .ToList();
        var isIdentityInsert = entityModel.HasIdentityInsert();
        var script = new StringBuilder();
        if (isIdentityInsert)
        {
            script.Append($"SET IDENTITY_INSERT {tableRef} ON;\n");
        }

        var safeEntities = entities.Select(entity =>
        {
            // Lazily evaluated to avoid unnecessary processing
            var entityType = entity.GetType();
            if (entityType != type)
            {
                throw new ArgumentException("All entities in seed must be of the same type");
            }
            return entity;
        });

        var context = new GenerationContext
        {
            TableRef = tableRef,
            Properties = includedProperties,
            Entities = safeEntities
        };
        // Could do composition instead of inheritance, but would require more boilerplate
        GenerateScript(script, context);

        if (isIdentityInsert)
        {
            script.Append($"\nSET IDENTITY_INSERT {tableRef} OFF;");
        }
        return script.ToString();
    }

    protected class GenerationContext {
        public string TableRef { get; init; }
        public List<IProperty> Properties { get; init; }
        public IEnumerable<object> Entities { get; init; }
    }

    protected abstract void GenerateScript(StringBuilder script, GenerationContext context);
}
