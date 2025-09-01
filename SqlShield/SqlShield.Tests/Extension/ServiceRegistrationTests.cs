using Microsoft.Extensions.DependencyInjection;
using SqlShield.Extension;
using SqlShield.Interface;
using Xunit;

namespace SqlShield.Tests.Extension
{
    public class ServiceRegistrationTests
    {
        [Fact]
        public void AddDatabaseServices_WithNullConnectionString_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            string connectionString = null;

            // Act & Assert
            // We assert that calling the method with a null string throws the correct exception.
            Assert.Throws<ArgumentNullException>(() =>
                services.AddDatabaseServices(connectionString));
        }

        [Fact]
        public void AddDatabaseServicesWithValidConnectionStringRegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var connectionString = "some_valid_connection_string";

            // Act
            services.AddDatabaseServices(connectionString);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // We ask the container for the service and assert that it's not null,
            // which proves it was registered successfully.
            var service = serviceProvider.GetService<IDatabaseService>();
            Assert.NotNull(service);
        }
    }
}
