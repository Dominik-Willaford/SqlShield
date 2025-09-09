using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SqlShield.Model
{
    public class ConventionMemberMap : SqlMapper.IMemberMap
    {
        public string ColumnName { get; }
        public PropertyInfo Property { get; }

        public ConventionMemberMap(string columnName, PropertyInfo property)
        {
            ColumnName = columnName;
            Property = property;
        }

        public string? Column => ColumnName;
        public Type MemberType => Property.PropertyType;
        public PropertyInfo? PropertyInfo => Property;
        public FieldInfo? Field => null;
        public ParameterInfo? Parameter => null;
    }
}
