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

        // Leaving this here so I can come back to add further items to the IDatabaseService
        public StoredProcedureExecutorService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string procedureName, string connectionString, Dictionary<string, object> parameters = null)
        {
            // The using is to ensure the connection is closed.
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                // Use Dapper to asynchronously execute the query.
                // Dapper automatically opens the connection, creates parameters, and maps the results to your class <T>.
                return await db.QueryAsync<T>(
                    procedureName,
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string procedureName, string connectionString, Dictionary<string, object> parameters = null)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return await db.ExecuteAsync(
                    procedureName,
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string procedureName, string connectionString, Dictionary<string, object> parameters = null)
        {
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
