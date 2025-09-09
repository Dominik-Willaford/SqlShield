using Dapper;
using Microsoft.Extensions.DependencyInjection;
using SqlShield.Extension;
using SqlShield.Service;
using Xunit;
using SqlShield.Tests.Model;

namespace SqlShield.Tests.Convention
{
    public class Setup : TypeMapBase
    {
        private class Dummy { public int FirstName { get; set; } }

        [Fact]
        public void AddSqlShieldWithSnakeCase_SetsGlobalProvider()
        {
            var services = new ServiceCollection();
            services.AddSqlShieldWithSnakeCase();

            var mapper = SqlMapper.TypeMapProvider(typeof(Dummy));
            Assert.IsType<ConventionTypeMapper>(mapper);
        }

        [Fact]
        public void AddSqlShieldWithKebabCase_SetsGlobalProvider()
        {
            var services = new ServiceCollection();
            services.AddSqlShieldWithKebabCase();

            var mapper = SqlMapper.TypeMapProvider(typeof(Dummy));
            Assert.IsType<ConventionTypeMapper>(mapper);
        }

        [Fact]
        public void GlobalSnakeCase_MapsColumnToProperty()
        {
            var services = new ServiceCollection();
            services.AddSqlShieldWithSnakeCase();

            var mapper = SqlMapper.TypeMapProvider(typeof(Dummy));
            var member = mapper.GetMember("first_name");
            Assert.NotNull(member);
            Assert.Equal("FirstName", member!.Property!.Name);
        }

        [Fact]
        public void GlobalKebabCase_MapsColumnToProperty()
        {
            var services = new ServiceCollection();
            services.AddSqlShieldWithKebabCase();

            var mapper = SqlMapper.TypeMapProvider(typeof(Dummy));
            var member = mapper.GetMember("first-name");
            Assert.NotNull(member);
            Assert.Equal("FirstName", member!.Property!.Name);
        }
    }
}
