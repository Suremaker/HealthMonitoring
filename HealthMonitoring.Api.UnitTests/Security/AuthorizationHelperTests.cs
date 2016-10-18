using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Security;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Security
{
    public class AuthorizationHelperTests
    {
        private const string _monitorId = "30494205-ae15-4de7-90ed-2705431a3914";

        [Theory]
        [InlineData(SecurityRole.Admin)]
        [InlineData(SecurityRole.Monitor)]
        public void IsSelfAuthorized_should_not_accept_admin_and_pullmonitor_credentials(SecurityRole role)
        {
            var context = GetRequestContext(role);
            Assert.Throws<UnauthorizedAccessException>(() => context.Authorize(Guid.NewGuid()));
        }

        [Fact]
        public void AuthorizeByRoles_should_throw_unauthorize_ex_if_principal_is_not_in_any_role()
        {
            var roles = new [] {SecurityRole.Admin};
            var context = GetRequestContext(roles);
            Assert.Throws<UnauthorizedAccessException>(() => context.Authorize(SecurityRole.Monitor));
        }

        [Fact]
        public void ParseAuthorizationHeader_should_decrypt_authorization_header_credentials()
        {
            Guid monitorId = Guid.NewGuid();
            string privateToken = "private_token";
            var auth = new AuthenticationHeaderValue("Basic", $"{monitorId}:{privateToken}".ToBase64String());
            var context = GetAuthorizedExceptionContext(auth);

            var credentials = context.ParseAuthorizationHeader();
            Assert.Equal(credentials.MonitorId, monitorId);
            Assert.Equal(credentials.PrivateToken, privateToken);
        }

        [Theory]
        [InlineData("")]
        [InlineData("id,token")]
        [InlineData(_monitorId)]
        [InlineData(_monitorId + ",")]
        public void ParseAuthorizationHeader_should_return_null_if_header_credentials_format_is_invalid(string authParameter)
        {
            var auth = new AuthenticationHeaderValue("Basic", authParameter.ToBase64String());
            var context = GetAuthorizedExceptionContext(auth);
            var credentials = context.ParseAuthorizationHeader();

            Assert.Null(credentials);
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

        private ExceptionHandlerContext GetAuthorizedExceptionContext(AuthenticationHeaderValue authHeader)
        {
            var ex = new Exception();
            var catchBlock = new ExceptionContextCatchBlock("catch", false, false);
            var context = new ExceptionHandlerContext(new ExceptionContext(ex, catchBlock));
            context.ExceptionContext.Request = new HttpRequestMessage();
            context.ExceptionContext.Request.Headers.Authorization = authHeader;
            return context;
        }
    }
}
