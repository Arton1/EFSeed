﻿using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core.StatementGenerators;

internal class SqlValuesListGenerator
{
    private readonly List<IProperty> _properties;

    public SqlValuesListGenerator(List<IProperty> properties)
    {
        _properties = properties;
    }

    public string Generate(object entity)
    {
        var script = new StringBuilder();
        script.Append("(");
        foreach (var property in _properties)
        {
            var value = entity.GetType().GetProperty(property.Name)?.GetValue(entity);
            script.Append($"{FormatValue(value, property)}, ");
        }
        script.Remove(script.Length - 2, 2);
        script.Append(")");
        return script.ToString();
    }

    // Have to generate SQL values instead of using parametrized queries
    private string FormatValue(object? value, IProperty property)
    {
        var convertedValue = ConvertValue(value, property);
        return convertedValue switch
        {
            null => "NULL",
            string text => $"N'{text.Replace("'", "''")}'",
            DateTime date => $"'{date:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            float d => d.ToString(CultureInfo.InvariantCulture),
            int i => i.ToString(CultureInfo.InvariantCulture),
            byte[] bArr => "0x" + BitConverter.ToString(bArr).Replace("-", ""),
            Guid g => $"'{g}'",
            TimeSpan ts => $"'{ts:c}'",
            _ => convertedValue.ToString()!
        };
    }

    // handles mainly enum types
    private object? ConvertValue(object? value, IProperty property) =>
        property.FindTypeMapping()?.Converter?.ConvertToProvider.Invoke(value) ?? value;
}
