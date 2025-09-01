using Dapper;
using SqlShield.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Service
{
    internal class QueryBuilder<T> : IQueryBuilder<T> where T : new()
    {
        private readonly DatabaseService _service;
        private readonly List<Expression<Func<T, bool>>> _wherePredicates = new();
        private readonly string _tableName;

        public QueryBuilder(DatabaseService service)
        {
            _service = service;
            _tableName = $"{typeof(T).Name}s"; // Convention
        }

        public IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            // 1. Simply add the expression to our list.
            _wherePredicates.Add(predicate);
            return this; // Return 'this' to allow chaining.
        }

        public IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            // Logic to parse the OrderBy expression
            return this;
        }

        public async Task<IEnumerable<T>> QueryAsync()
        {
            // 2. Build and validate the SQL just before execution.
            var (sql, parameters) = await BuildAndValidateSqlAsync();

            // 3. Execute the final, validated query.
            using (var connection = _service.CreateConnection())
            {
                return await connection.QueryAsync<T>(sql, parameters);
            }
        }

        public async Task<T> QuerySingleAsync()
        {
            var (sql, parameters) = await BuildAndValidateSqlAsync();
            using (var connection = _service.CreateConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<T>(sql);
            }
        }

        /// <summary>
        /// Parses a simple binary expression (e.g., u => u.Id == 5) to extract the
        /// property name and the value.
        /// </summary>
        private (string MemberName, object Value) ParseBinaryExpression(Expression<Func<T, bool>> predicate)
        {
            // The body of the lambda expression (e.g., u.Id == 5)
            if (predicate.Body is not BinaryExpression binaryExpr)
            {
                throw new NotSupportedException("Only simple binary expressions are supported in Where clauses.");
            }

            // We need to find the side of the expression that is the Member (u.Id)
            // and which side is the Constant (5).
            MemberExpression memberExpr;
            ConstantExpression constantExpr;

            if (binaryExpr.Left is MemberExpression leftMember && binaryExpr.Right is ConstantExpression rightConstant)
            {
                memberExpr = leftMember;
                constantExpr = rightConstant;
            }
            else if (binaryExpr.Right is MemberExpression rightMember && binaryExpr.Left is ConstantExpression leftConstant)
            {
                memberExpr = rightMember;
                constantExpr = leftConstant;
            }
            else
            {
                // This could be enhanced to support more complex scenarios, like comparing two properties
                throw new NotSupportedException("Expression must compare a property to a constant value.");
            }

            string memberName = memberExpr.Member.Name;
            object value = constantExpr.Value;

            return (memberName, value);
        }

        public async Task<(string Sql, DynamicParameters Parameters)> BuildAndValidateSqlAsync()
        {
            var parameters = new DynamicParameters();
            var sqlBuilder = new StringBuilder($"SELECT * FROM {_tableName}");

            if (_wherePredicates.Any())
            {
                sqlBuilder.Append(" WHERE ");
                using (var connection = _service.CreateConnection())
                {
                    // Get the schema ONCE for all validations in this query.
                    var schema = await _service.GetSchemaCache().GetTableSchemaAsync(typeof(T), connection);

                    for (int i = 0; i < _wherePredicates.Count; i++)
                    {
                        var predicate = _wherePredicates[i];
                        var (memberName, value) = ParseBinaryExpression(predicate); // Your existing helper

                        // Validate the member against the cached schema
                        if (!schema.Columns.Contains(memberName))
                        {
                            throw new InvalidOperationException($"Column '{memberName}' not found on table '{schema.TableName}'.");
                        }

                        var paramName = $"@p{i}";
                        sqlBuilder.Append($"{(i > 0 ? " AND " : "")}[{memberName}] = {paramName}");
                        parameters.Add(paramName, value);
                    }
                }
            }

            return (sqlBuilder.ToString(), parameters);
        }
    }
}
