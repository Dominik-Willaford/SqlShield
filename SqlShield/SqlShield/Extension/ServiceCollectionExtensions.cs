using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlShield.Interface;
using SqlShield.Model;
using SqlShield.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Extension
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers SqlShield convention mappers for all classes in the provided assembly.
        /// </summary>
        public static IServiceCollection AddSqlShield(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                // Only apply if class has [DapperConvention] OR at least one [ColumnOverride]
                var hasConventionAttr = type.GetCustomAttribute<DapperConventionAttribute>() != null;
                var hasOverrideAttr = type.GetProperties()
                    .Any(p => p.GetCustomAttribute<ColumnOverrideAttribute>() != null);

                if (hasConventionAttr || hasOverrideAttr)
                {
                    SqlMapper.SetTypeMap(type, new ConventionTypeMapper(type));
                }
            }

            return services;
        }

        /// <summary>
        /// Registers SqlShield convention mappers for all classes in the assembly containing T.
        /// </summary>
        public static IServiceCollection AddSqlShield<T>(this IServiceCollection services)
        {
            return services.AddSqlShield(typeof(T).Assembly);
        }

        //// <summary>
        /// Enables SqlShield with snake_case global mapping convention.
        /// </summary>
        public static IServiceCollection AddSqlShieldWithSnakeCase(this IServiceCollection services)
        {
            var converter = new SnakeCaseConverter();
            SqlMapper.TypeMapProvider = type => new ConventionTypeMapper(type, converter);
            return services;
        }

        /// <summary>
        /// Enables SqlShield with kebab-case global mapping convention.
        /// </summary>
        public static IServiceCollection AddSqlShieldWithKebabCase(this IServiceCollection services)
        {
            var converter = new KebabCaseConverter();
            SqlMapper.TypeMapProvider = type => new ConventionTypeMapper(type, converter);
            return services;
        }

        /// <summary>
        /// Enables SqlShield with no global convention.
        /// Falls back to DefaultTypeMap unless a class-level [DapperConvention] is applied.
        /// </summary>
        public static IServiceCollection AddSqlShieldWithoutConvention(this IServiceCollection services)
        {
            SqlMapper.TypeMapProvider = type => new ConventionTypeMapper(type, null);
            return services;
        }
    }
}
