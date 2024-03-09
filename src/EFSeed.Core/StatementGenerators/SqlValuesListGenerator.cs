using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core.StatementGenerators;

public class SqlValuesListGenerator
{
    private readonly List<IProperty> _properties;

    public SqlValuesListGenerator(IEntityType entityModel)
    {
        _properties = entityModel.GetProperties().ToList();

    }

    public string Generate(object entity)
    {
        var script = new StringBuilder();
        script.Append("(");
        foreach (var property in _properties)
        {
            var value = entity.GetType().GetProperty(property.Name)?.GetValue(entity);
            script.Append($"{FormatValue(value)}, ");
        }
        script.Remove(script.Length - 2, 2);
        script.Append(")");
        return script.ToString();
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
