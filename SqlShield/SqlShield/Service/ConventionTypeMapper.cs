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
    internal class ConventionTypeMapper : IConventionTypeMapper
    {
        private readonly Dictionary<string, PropertyInfo> _columnMappings;
        private readonly DefaultTypeMap _defaultMap;

        /// <summary>
        /// Initializes a new instance of the ConventionTypeMapper.
        /// </summary>
        /// <param name="type">The type to map to.</param>
        /// <param name="columnNameConverter">An instance of a class that implements INameConventionConverter.</param>
        public ConventionTypeMapper(Type type, INameConventionConverter columnNameConverter)
        {
            _columnMappings = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            _defaultMap = new DefaultTypeMap(type);

            // This is the correct approach: build the map in the constructor.
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                // First, map the property name directly (in case it's a perfect match).
                _columnMappings[property.Name] = property;

                // Then, use the converter to get the database column name and map it as well.
                string databaseColumnName = columnNameConverter.Convert(property.Name);
                if (!string.IsNullOrEmpty(databaseColumnName))
                {
                    _columnMappings[databaseColumnName] = property;
                }
            }
        }

        /// <summary>
        /// Gets the constructor for a given set of names and types.
        /// Delegates to the underlying DefaultTypeMap.
        /// </summary>
        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _defaultMap.FindConstructor(names, types);
        }

        /// <summary>
        /// Gets an explicit constructor.
        /// Delegates to the underlying DefaultTypeMap.
        /// </summary>
        public ConstructorInfo FindExplicitConstructor()
        {
            return _defaultMap.FindExplicitConstructor();
        }

        /// <summary>
        /// Gets the property for a given column name. This is the core of our custom logic.
        /// </summary>
        /// <param name="columnName">The name of the database column.</param>
        /// <returns>The PropertyInfo object for the matching property, or null if no match is found.</returns>
        public PropertyInfo GetProperty(string columnName)
        {
            if (_columnMappings.TryGetValue(columnName, out var property))
            {
                return property;
            }

            return null;
        }

        /// <summary>
        /// Gets the member map for a given column name. This is the primary method Dapper calls for mapping.
        /// </summary>
        public IMemberMap GetMember(string columnName)
        {
            var property = GetProperty(columnName);
            if (property != null)
            {
                // Now, we use our custom ConventionMemberMap to return a valid IMemberMap instance.
                return new ConventionMemberMap(columnName, property);
            }

            // If our custom mapping doesn't find a match, defer to the default map.
            return _defaultMap.GetMember(columnName);
        }

        /// <summary>
        /// Gets the member map for a given constructor parameter.
        /// </summary>
        public IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            return _defaultMap.GetConstructorParameter(constructor, columnName);
        }
    }
}
