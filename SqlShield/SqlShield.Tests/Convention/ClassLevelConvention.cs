using Dapper;
using SqlShield.Model;
using SqlShield.Service;
using SqlShield.Tests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Tests.Convention
{
    public class ClassLevelConvention : TypeMapBase
    {
        [DapperConvention(typeof(SnakeCaseConverter))]
        private class WithSnake
        {
            public string? FirstName { get; set; }
        }

        [DapperConvention(typeof(KebabCaseConverter))]
        private class WithKebab
        {
            public string? FirstName { get; set; }
        }

        [Fact]
        public void ClassSnake_MapsSnake_EvenIfGlobalKebab()
        {
            SqlMapper.TypeMapProvider = t => new ConventionTypeMapper(t, new KebabCaseConverter());
            var mapper = SqlMapper.TypeMapProvider(typeof(WithSnake));
            var member = mapper.GetMember("first_name");
            Assert.NotNull(member);
            Assert.Equal("FirstName", member!.Property!.Name);
        }

        [Fact]
        public void ClassKebab_MapsKebab_EvenIfGlobalSnake()
        {
            SqlMapper.TypeMapProvider = t => new ConventionTypeMapper(t, new SnakeCaseConverter());
            var mapper = SqlMapper.TypeMapProvider(typeof(WithKebab));
            var member = mapper.GetMember("first-name");
            Assert.NotNull(member);
            Assert.Equal("FirstName", member!.Property!.Name);
        }
    }
}
