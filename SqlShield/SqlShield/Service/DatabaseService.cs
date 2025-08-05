using SqlShield.Interface;
using SqlShield.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SqlShield.Service
{
    internal class DatabaseService : IDatabaseService
    {
        private readonly SqlShieldSettings _sqlShieldSettings;
        private readonly ICryptography _cryptoService;

        public DatabaseService(IOptions<SqlShieldSettings> sqlShieldSettings, ICryptography cryptoService)
        {
            _sqlShieldSettings = sqlShieldSettings.Value;
            _cryptoService = cryptoService;
        }

        public string GetConnectionString(string connectionName)
        {
            if (!_sqlShieldSettings.Connections.TryGetValue(connectionName, out var connection))
            {
                throw new ArgumentException($"Connection name '{connectionName}' not found in configuration.");
            }

            string connString = _cryptoService.BuildConnString(connection.ConnectionString, connection.ConnectionPassword);

            return connString;
        }


    }
}
