using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace SqlShield.Model
{
    public class ConventionMemberMap : IMemberMap
    {
        public string ColumnName { get; }
        public Type MemberType { get; }
        public PropertyInfo Property { get; }
        public FieldInfo Field => null; // Not supported
        public ParameterInfo Parameter => null; // Not supported

        public ConventionMemberMap(string columnName, PropertyInfo property)
        {
            ColumnName = columnName;
            Property = property;
            MemberType = property.PropertyType;
        }
    }
}
