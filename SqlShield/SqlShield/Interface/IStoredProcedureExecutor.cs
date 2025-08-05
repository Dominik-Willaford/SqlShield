using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Interface
{
    public interface IStoredProcedureExecutor
    {
        /// <summary>
        /// Executes a stored procedure that returns a list of results (e.g., a SELECT statement).
        /// </summary>
        /// <typeparam name="T">The C# class to map the results to.</typeparam>
        Task<IEnumerable<T>> QueryAsync<T>(string procedureName, string connectionName, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Executes a stored procedure that does not return a result set (e.g., INSERT, UPDATE, DELETE).
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        Task<int> ExecuteNonQueryAsync(string procedureName, string connectionName, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Executes a stored procedure that returns a single value.
        /// </summary>
        /// <typeparam name="T">The type of the single value to be returned.</typeparam>
        Task<T> ExecuteScalarAsync<T>(string procedureName, string connectionName, Dictionary<string, object> parameters = null);
    }
}
