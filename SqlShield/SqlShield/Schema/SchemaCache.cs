using Dapper;
using System.Collections.Concurrent;
using System.Data;

namespace SqlShield.Schema
{
    public class SchemaCache
    {
        private readonly ConcurrentDictionary<Type, TableSchema> _cache = new();

        /// <summary>
        /// Gets the schema for a given type, fetching it from the database if not cached.
        /// </summary>
        public virtual async Task<TableSchema> GetTableSchemaAsync(Type type, IDbConnection connection)
        {
            // 1. Check the cache first. This is the fast path.
            if (_cache.TryGetValue(type, out var schema))
            {
                return schema;
            }

            // 2. Cache Miss: Query the database for the schema.
            // We'll assume the table name is the class name + 's' (e.g., User -> Users)
            // This convention can be made more robust later.
            var tableName = $"{type.Name}s";

            var sql = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @tableName";

            var columnNames = await connection.QueryAsync<string>(sql, new { tableName });

            if (!columnNames.Any())
            {
                throw new InvalidOperationException($"Table '{tableName}' not found or has no columns.");
            }

            // 3. Create the schema object and store it in the cache for next time.
            var newSchema = new TableSchema(tableName, columnNames);
            _cache.TryAdd(type, newSchema);

            return newSchema;
        }
    }
}
