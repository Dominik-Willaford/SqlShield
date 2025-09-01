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
}
