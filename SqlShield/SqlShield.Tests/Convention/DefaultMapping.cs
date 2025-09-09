using Dapper;
using SqlShield.Service;
using SqlShield.Tests.Model;
using System.Data;
using Xunit;

namespace SqlShield.Tests.Convention
{
    public class DefaultMapping :TypeMapBase
    {
        private class Plain
        {
            public int Id { get; set; }
            public string? FirstName { get; set; }
        }

        [Fact]
        public void NoGlobal_NoAttributes_DefaultDoesNotMapSnake()
        {
            // No global provider
            var defaultMap = new DefaultTypeMap(typeof(Plain));
            var member = defaultMap.GetMember("first_name"); // default looks for exact match
            Assert.Null(member);
        }

        [Fact]
        public void ConventionTypeMapper_NoConverter_FallsBackToDefault()
        {
            var mapper = new ConventionTypeMapper(typeof(Plain), globalConverter: null);
            var member = mapper.GetMember("first_name");
            Assert.Null(member); // no converter => same as default behavior
        }
    }
}
