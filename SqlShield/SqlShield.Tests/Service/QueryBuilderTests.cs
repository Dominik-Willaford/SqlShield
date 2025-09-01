using Moq;
using SqlShield.Schema;
using SqlShield.Service;
using SqlShield.Tests.Model;
using System.Data;
using System.Linq.Expressions;
using Xunit;

namespace SqlShield.Tests.Service
{
    public class QueryBuilderTests
    {
        private readonly Mock<DatabaseService> _mockDbService;
        private readonly Mock<SchemaCache> _mockSchemaCache;
        private readonly Mock<IDbConnection> _mockDbConnection;

        public QueryBuilderTests()
        {
            // We mock the dependencies so we don't need a real database.
            _mockDbConnection = new Mock<IDbConnection>();
            _mockSchemaCache = new Mock<SchemaCache>();

            // Create a mock of the DatabaseService. Because its methods are virtual,
            // Moq can override their behavior.
            _mockDbService = new Mock<DatabaseService>("fake_connection_string");

            // Setup the mocks to return our fake objects.
            _mockDbService.Setup(s => s.CreateConnection()).Returns(_mockDbConnection.Object);
            _mockDbService.Setup(s => s.GetSchemaCache()).Returns(_mockSchemaCache.Object);
        }

        [Fact]
        public async Task QueryAsync_WithSimpleWhereClause_GeneratesCorrectSql()
        {
            // Arrange
            var userSchema = new TableSchema("Users", new[] { "Id", "Name" });
            Expression<Func<User, bool>> predicate = u => u.Id == 123;

            // Tell our fake SchemaCache to return our fake schema.
            _mockSchemaCache
                .Setup(c => c.GetTableSchemaAsync(typeof(User), _mockDbConnection.Object))
                .ReturnsAsync(userSchema);

            var queryBuilder = new QueryBuilder<User>(_mockDbService.Object);

            // Act
            queryBuilder.Where(predicate);

            // To test the generated SQL, we need a way to get it. Let's assume
            // BuildAndValidateSqlAsync is made internal for testing.
            var (sql, parameters) = await queryBuilder.BuildAndValidateSqlAsync();

            // Assert
            Assert.Equal("SELECT * FROM Users WHERE [Id] = @p0", sql);
            Assert.True(parameters.ParameterNames.Contains("p0"));
            Assert.Equal(123, parameters.Get<int>("p0"));
        }
    }
}
