using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SqlShield.Interface;
using SqlShield.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Service
{
    internal class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>Factory only; caller must open & dispose.</summary>
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        /// <summary>Convenience: returns an OPEN connection; caller must dispose.</summary>
        public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            return conn;
        }

        /// <summary>Executes a function with an open connection; handles open/close/dispose.</summary>
        public async Task<T> WithConnectionAsync<T>(
            Func<IDbConnection, Task<T>> work,
            CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            return await work(conn);
        }
    }
}
