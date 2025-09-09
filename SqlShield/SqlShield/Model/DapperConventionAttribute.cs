using SqlShield.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Model
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DapperConventionAttribute : Attribute
    {
        public Type ConverterType { get; }

        public DapperConventionAttribute(Type converterType)
        {
            if (!typeof(INameConventionConverter).IsAssignableFrom(converterType))
            {
                throw new ArgumentException($"Converter type must implement {nameof(INameConventionConverter)}.", nameof(converterType));
            }
            ConverterType = converterType;
        }
    }

    /// <summary>
    /// Explicitly map a property to a database column name.
    /// This always takes priority over conventions (global or class-level).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnOverrideAttribute : Attribute
    {
        public string ColumnName { get; }

        public ColumnOverrideAttribute(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name cannot be null or whitespace.", nameof(columnName));

            ColumnName = columnName;
        }
    }
}
