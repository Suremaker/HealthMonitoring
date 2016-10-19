using System;
using HealthMonitoring.Security;
using Xunit;

namespace HealthMonitoring.UnitTests
{
    public class SecurityTests
    {
        [Fact]
        public void Hashcodes_of_the_same_credentials_should_be_equal()
        {
            Guid id = Guid.NewGuid();
            string password = "password";
            var cred1 = new Credentials(id, password);
            var cred2 = new Credentials(id, password);

            Assert.Equal(cred1.GetHashCode(), cred2.GetHashCode());
        }

        [Fact]
        public void Hashcodes_of_credentials_with_different_parameters_should_be_not_equal()
        {
            Assert.NotEqual(new Credentials(Guid.NewGuid(), "same").GetHashCode(), new Credentials(Guid.NewGuid(), "same").GetHashCode());
            Assert.NotEqual(new Credentials(Guid.Empty, "first").GetHashCode(), new Credentials(Guid.Empty, "second").GetHashCode());
            Assert.NotEqual(new Credentials(Guid.NewGuid(), "first").GetHashCode(), new Credentials(Guid.NewGuid(), "second").GetHashCode());
        }
    }
}
