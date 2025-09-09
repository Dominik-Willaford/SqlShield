using Dapper;
using SqlShield.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlShield.Service
{
    internal sealed class QueryBuilder<T> : IQueryBuilder<T> where T : new()
    {
        private readonly DatabaseService _service;
        private readonly List<Expression<Func<T, bool>>> _wherePredicates = new();
        private readonly List<(LambdaExpression keySelector, bool descending)> _orderings = new();
        private readonly string _tableName;

        public QueryBuilder(DatabaseService service)
        {
            _service = service;
            _tableName = DefaultPluralizedTable(typeof(T).Name); // convention
        }

        public IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            _wherePredicates.Add(predicate);
            return this;
        }

        public IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _orderings.Add((keySelector, false));
            return this;
        }

        public IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _orderings.Add((keySelector, true));
            return this;
        }

        public async Task<IEnumerable<T>> QueryAsync()
        {
            var (sql, parameters) = await BuildAndValidateSqlAsync();
            using var connection = _service.CreateConnection();
            connection.Open();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<T> QuerySingleAsync()
        {
            var (sql, parameters) = await BuildAndValidateSqlAsync();
            using var connection = _service.CreateConnection();
            connection.Open();
            return await connection.QuerySingleAsync<T>(sql, parameters);
        }

        public async Task<(string Sql, DynamicParameters Parameters)> BuildAndValidateSqlAsync()
        {
            await Task.Yield(); // keep async-friendly, no validation

            var parameters = new DynamicParameters();
            var sb = new StringBuilder($"SELECT * FROM {QuoteIdentifier(_tableName)}");

            if (_wherePredicates.Any())
            {
                var parts = new List<string>();
                for (int i = 0; i < _wherePredicates.Count; i++)
                {
                    parts.Add(ParsePredicateToSql(_wherePredicates[i], parameters, i));
                }
                sb.Append(" WHERE ").Append(string.Join(" AND ", parts));
            }

            if (_orderings.Any())
            {
                var orderParts = _orderings.Select(o =>
                {
                    var propName = GetMemberName(o.keySelector);
                    var dir = o.descending ? "DESC" : "ASC";
                    return $"{QuoteIdentifier(propName)} {dir}";
                });
                sb.Append(" ORDER BY ").Append(string.Join(", ", orderParts));
            }

            return (sb.ToString(), parameters);
        }

        // ---------- Helpers ----------

        private static string DefaultPluralizedTable(string name)
        {
            if (name.EndsWith("y", StringComparison.Ordinal)) return name[..^1] + "ies";
            if (name.EndsWith("s", StringComparison.Ordinal)) return name + "es";
            return name + "s";
        }

        private string ParsePredicateToSql(LambdaExpression predicate, DynamicParameters p, int indexBase)
        {
            return predicate.Body switch
            {
                BinaryExpression be => ParseBinary(be, p, indexBase),
                MethodCallExpression mce => ParseStringMethod(mce, p, indexBase),
                _ => throw new NotSupportedException($"Unsupported predicate: {predicate.Body.NodeType}")
            };
        }

        private string ParseBinary(BinaryExpression be, DynamicParameters p, int indexBase)
        {
            (MemberExpression member, Expression other, ExpressionType op) = be.Left switch
            {
                MemberExpression m when IsColumnCandidate(m) => (m, be.Right, be.NodeType),
                _ when be.Right is MemberExpression m2 && IsColumnCandidate(m2) => (m2, be.Left, FlipOperator(be.NodeType)),
                _ => throw new NotSupportedException("Binary Where requires a member vs constant/expression.")
            };

            var columnName = member.Member.Name;
            var quotedColumn = QuoteIdentifier(columnName);

            var value = Evaluate(other);
            if (value is null || value == DBNull.Value)
            {
                return op switch
                {
                    ExpressionType.Equal => $"{quotedColumn} IS NULL",
                    ExpressionType.NotEqual => $"{quotedColumn} IS NOT NULL",
                    _ => throw new NotSupportedException("Only == and != supported for NULL.")
                };
            }

            var paramName = $"p{indexBase}";
            p.Add(paramName, value);

            var opSql = op switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                _ => throw new NotSupportedException($"Operator {op} not supported.")
            };

            return $"{quotedColumn} {opSql} @{paramName}";
        }

        private string ParseStringMethod(MethodCallExpression mce, DynamicParameters p, int indexBase)
        {
            if (mce.Object is not MemberExpression member || !IsColumnCandidate(member))
                throw new NotSupportedException("Supported string methods must be called on a mapped property.");

            var arg = Evaluate(mce.Arguments[0]);
            var columnName = member.Member.Name;
            var quotedColumn = QuoteIdentifier(columnName);
            var paramName = $"p{indexBase}";

            return mce.Method.Name switch
            {
                nameof(string.Contains) => AddLike(p, paramName, $"%{arg}%", $"{quotedColumn} LIKE @{paramName}"),
                nameof(string.StartsWith) => AddLike(p, paramName, $"{arg}%", $"{quotedColumn} LIKE @{paramName}"),
                nameof(string.EndsWith) => AddLike(p, paramName, $"%{arg}", $"{quotedColumn} LIKE @{paramName}"),
                _ => throw new NotSupportedException($"String method {mce.Method.Name} not supported.")
            };

            static string AddLike(DynamicParameters p, string name, object? val, string sql)
            { p.Add(name, val); return sql; }
        }

        private static bool IsColumnCandidate(MemberExpression m)
            => m.Member.MemberType == System.Reflection.MemberTypes.Property && m.Expression is ParameterExpression;

        private static string GetMemberName(LambdaExpression lambda)
        {
            if (lambda.Body is MemberExpression me && me.Member.MemberType == System.Reflection.MemberTypes.Property)
                return me.Member.Name;
            throw new NotSupportedException("OrderBy key selector must be a simple property access.");
        }

        private static object? Evaluate(Expression expr)
        {
            if (expr is ConstantExpression c) return c.Value;
            var obj = Expression.Convert(expr, typeof(object));
            var getter = Expression.Lambda<Func<object?>>(obj);
            return getter.Compile().Invoke();
        }

        private static string QuoteIdentifier(string ident)
            => ident.StartsWith("[") && ident.EndsWith("]") ? ident : $"[{ident}]";

        private static ExpressionType FlipOperator(ExpressionType op) => op switch
        {
            ExpressionType.GreaterThan => ExpressionType.LessThan,
            ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
            ExpressionType.LessThan => ExpressionType.GreaterThan,
            ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
            _ => op
        };
    }
}