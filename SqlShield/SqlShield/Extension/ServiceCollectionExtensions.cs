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
        /// Registers a custom Dapper TypeMap for a given type and naming convention converter.
        /// </summary>
        public static IServiceCollection RegisterDapperConvention<T>(this IServiceCollection services, INameConventionConverter converter)
        {
            SqlMapper.SetTypeMap(typeof(T), new ConventionTypeMapper(typeof(T), converter));
            return services;
        }

        /// <summary>
        /// Scans a given assembly for types with the DapperConventionAttribute and registers them.
        /// This removes the need for manual, per-type registration.
        /// </summary>
        public static IServiceCollection AddDapperConventionsFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            var typesToRegister = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Type = t,
                    Attribute = t.GetCustomAttribute<DapperConventionAttribute>()
                })
                .Where(x => x.Attribute != null);

            foreach (var item in typesToRegister)
            {
                var converter = (INameConventionConverter)Activator.CreateInstance(item.Attribute.ConverterType);
                SqlMapper.SetTypeMap(item.Type, new ConventionTypeMapper(item.Type, converter));
            }

            return services;
        }

        public static IServiceCollection AddDatabaseServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            // This registers your service. When a constructor asks for IDatabaseService,
            // the DI container will provide an instance of DatabaseService.
            services.AddScoped<IDatabaseService>(sp => new DatabaseService(connectionString));
            services.AddScoped<IStoredProcedureExecutor>(sp => new StoredProcedureExecutorService(sp.GetRequiredService<IDatabaseService>()));

            // We can now use our DapperConventionManager logic to register the mappings.
            // This happens once at application startup.
            services.AddDapperConventionsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

            return services;
        }
    }
}
