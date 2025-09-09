using Dapper;
using SqlShield.Interface;
using SqlShield.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SqlShield.Service
{
    /// <summary>
    /// Custom type mapper that applies column-to-property mapping conventions.
    /// Priority:
    /// 1. ColumnOverrideAttribute (per property)
    /// 2. DapperConventionAttribute (per class)
    /// 3. Global convention (set via SqlMapper.TypeMapProvider)
    /// 4. DefaultTypeMap (Dapper default)
    /// </summary>
    internal class ConventionTypeMapper : SqlMapper.ITypeMap
    {
        private readonly SqlMapper.ITypeMap _defaultTypeMap;
        private readonly INameConventionConverter? _converter;
        private readonly PropertyInfo[] _properties;

        public ConventionTypeMapper(Type type, INameConventionConverter? globalConverter = null)
        {
            _defaultTypeMap = new DefaultTypeMap(type);
            _properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Class-level convention (wins over global if defined)
            var conventionAttr = type.GetCustomAttribute<DapperConventionAttribute>();
            if (conventionAttr != null)
            {
                _converter = (INameConventionConverter)Activator.CreateInstance(conventionAttr.ConverterType)!;
            }
            else
            {
                _converter = globalConverter;
            }
        }

        public ConstructorInfo? FindConstructor(string[] names, Type[] types)
            => _defaultTypeMap.FindConstructor(names, types);

        public ConstructorInfo? FindExplicitConstructor()
            => _defaultTypeMap.FindExplicitConstructor();

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
            => _defaultTypeMap.GetConstructorParameter(constructor, columnName);

        public SqlMapper.IMemberMap? GetMember(string columnName)
        {
            foreach (var prop in _properties)
            {
                // 1. ColumnOverrideAttribute wins
                var overrideAttr = prop.GetCustomAttribute<ColumnOverrideAttribute>();
                if (overrideAttr != null &&
                    string.Equals(overrideAttr.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return new ConventionMemberMap(columnName, prop);
                }

                // 2. Apply converter (class-level or global)
                if (_converter != null)
                {
                    var convertedName = _converter.Convert(columnName);
                    if (string.Equals(convertedName, prop.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return new ConventionMemberMap(columnName, prop);
                    }
                }
            }

            // 3. Fallback to default Dapper logic
            return _defaultTypeMap.GetMember(columnName);
        }
    }
}
