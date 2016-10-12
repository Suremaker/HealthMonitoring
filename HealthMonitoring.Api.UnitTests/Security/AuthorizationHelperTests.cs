using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Http.Controllers;
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Security;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Security
{
    public class AuthorizationHelperTests
    {
        [Theory]
        [InlineData(SecurityRole.AdminMonitor)]
        [InlineData(SecurityRole.PullMonitor)]
        public void IsSelfAuthorized_should_not_accept_admin_and_pullmonitor_credentials(SecurityRole role)
        {
            var context = GetRequestContext(role);
            Assert.Throws<UnauthorizedAccessException>(() => context.Authorize(Guid.NewGuid()));
        }

        [Fact]
        public void AuthorizeByRoles_should_throw_unauthorize_ex_if_principal_is_not_in_any_role()
        {
            var roles = new [] {SecurityRole.AdminMonitor};
            var context = GetRequestContext(roles);
            Assert.Throws<UnauthorizedAccessException>(() => context.Authorize(SecurityRole.PullMonitor));
        }

        private HttpRequestContext GetRequestContext(params SecurityRole[] roles)
        {
            var identity = new GenericIdentity(Guid.NewGuid().ToString());
            var principal = new GenericPrincipal(identity, roles.Select(m => m.ToString()).ToArray());
            return new HttpRequestContext
            {
                Principal = principal
            };
        }
    }
}
