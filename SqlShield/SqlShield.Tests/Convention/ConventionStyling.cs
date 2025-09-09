using SqlShield.Service;
using Xunit;

namespace SqlShield.Tests.Convention
{
    public class ConventionStyling
    {
        [Fact]
        public void SnakeCaseConverter_Works()
        {
            var c = new SnakeCaseConverter();
            Assert.Equal("FirstName", c.Convert("first_name"));
            Assert.Equal("UserId", c.Convert("user_id"));
            Assert.Equal("X", c.Convert("x"));
        }

        [Fact]
        public void KebabCaseConverter_Works()
        {
            var c = new KebabCaseConverter();
            Assert.Equal("FirstName", c.Convert("first-name"));
            Assert.Equal("UserId", c.Convert("user-id"));
            Assert.Equal("X", c.Convert("x"));
        }
    }
}
