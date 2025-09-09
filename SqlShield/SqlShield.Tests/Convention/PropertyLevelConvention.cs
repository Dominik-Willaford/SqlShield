using Dapper;
using SqlShield.Model;
using SqlShield.Service;
using SqlShield.Tests.Model;
using Xunit;

namespace SqlShield.Tests.Convention
{
    public class PropertyLevelConvention : TypeMapBase
    {
        private class Overrides
        {
            [ColumnOverride("id")]
            public int UserId { get; set; }

            [ColumnOverride("first_name")]
            public string? GivenName { get; set; }
        }

        [Fact]
        public void ColumnOverride_WinsOverGlobalSnake()
        {
            SqlMapper.TypeMapProvider = t => new ConventionTypeMapper(t, new SnakeCaseConverter());

            var mapper = SqlMapper.TypeMapProvider(typeof(Overrides));
            Assert.Equal("UserId", mapper.GetMember("id")!.Property!.Name);
            Assert.Equal("GivenName", mapper.GetMember("first_name")!.Property!.Name);
        }

        [Fact]
        public void ColumnOverride_WinsOverGlobalKebab()
        {
            SqlMapper.TypeMapProvider = t => new ConventionTypeMapper(t, new KebabCaseConverter());

            var mapper = SqlMapper.TypeMapProvider(typeof(Overrides));
            Assert.Equal("UserId", mapper.GetMember("id")!.Property!.Name);
            Assert.Equal("GivenName", mapper.GetMember("first_name")!.Property!.Name); // not kebab; override exact
        }
    }
}
