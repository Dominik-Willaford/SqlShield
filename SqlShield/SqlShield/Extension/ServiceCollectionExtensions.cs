using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                   IConfiguration configuration)
        {
            // This line reads the "SqlShield" section from the parent's appsettings.json
            // and makes it available for injection.
            services.Configure<SqlShieldSettings>(configuration.GetSection("SqlShield"));

            // This registers your service. When a constructor asks for IDatabaseService,
            // the DI container will provide an instance of DatabaseService.
            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<ICryptography, CryptographyService>();
            services.AddScoped<IStoredProcedureExecutor, StoredProcedureExecutorService>();

            return services;
        }
    }
}
