using System.Globalization;
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

    // Have to generate SQL values instead of using parametrized queries
    private string FormatValue(object value) =>
        value switch
        {
            null => "NULL",
            string text => $"'{text.Replace("'", "''")}'",
            DateTime date => $"'{date:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            float d => d.ToString(CultureInfo.InvariantCulture),
            int i => i.ToString(CultureInfo.InvariantCulture),
            byte[] bArr => "0x" + BitConverter.ToString(bArr).Replace("-", ""),
            Guid g => $"'{g}'",
            TimeSpan ts => $"'{ts:c}'",
            _ => value.ToString()!
        };
}
