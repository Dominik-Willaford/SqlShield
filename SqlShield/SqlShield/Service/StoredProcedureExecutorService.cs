using Dapper;
using Microsoft.Data.SqlClient;
using SqlShield.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Service
{
    internal class StoredProcedureExecutorService : IStoredProcedureExecutor
    {
        private readonly IDatabaseService _databaseService;

        // We inject the service that knows how to build connection strings.
        public StoredProcedureExecutorService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string procedureName, string connectionName, Dictionary<string, object> parameters = null)
        {
            // 1. Get the fully decrypted connection string from our existing service.
            string connectionString = _databaseService.GetConnectionString(connectionName);

            // 2. Use a 'using' block to ensure the connection is closed.
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                // 3. Use Dapper to asynchronously execute the query.
                // Dapper automatically opens the connection, creates parameters, and maps the results to your class <T>.
                return await db.QueryAsync<T>(
                    procedureName,
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string procedureName, string connectionName, Dictionary<string, object> parameters = null)
        {
            string connectionString = _databaseService.GetConnectionString(connectionName);
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return await db.ExecuteAsync(
                    procedureName,
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string procedureName, string connectionName, Dictionary<string, object> parameters = null)
        {
            string connectionString = _databaseService.GetConnectionString(connectionName);
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return await db.ExecuteScalarAsync<T>(
                    procedureName,
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}
