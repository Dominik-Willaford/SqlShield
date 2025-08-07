using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlShield.Interface;
using SqlShield.Model;
using SqlShield.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(
                   this IServiceCollection services,
                   IConfiguration configuration,
                   string encryptionKey,
                   int iterations = 100000)
        {
            // This line reads the "SqlShield" section from the parent's appsettings.json
            // and makes it available for injection.
            services.Configure<SqlShieldSettings>(configuration.GetSection("SqlShield"));

            // This registers your service. When a constructor asks for IDatabaseService,
            // the DI container will provide an instance of DatabaseService.
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IStoredProcedureExecutor, StoredProcedureExecutorService>();
            
            /* Registering using a factory function to tell the dependency injection how to 
             * build the service.
             */
            services.AddScoped<ICryptography>(provider =>
            {
                return new CryptographyService(encryptionKey, iterations);
            });


            return services;
        }
    }
}
