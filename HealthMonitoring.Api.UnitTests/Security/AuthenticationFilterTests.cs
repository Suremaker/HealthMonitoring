using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Model;
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Security;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Api.UnitTests.Security
{
    public class AuthenticationFilterTests
    {
        private readonly Mock<IEndpointRegistry> _endpointRegistryMock = new Mock<IEndpointRegistry>();
        private readonly Mock<ICredentialsProvider> _tokenProviderMock = new Mock<ICredentialsProvider>();
        private readonly Mock<ITimeCoordinator> _timeCoordinatorMock = new Mock<ITimeCoordinator>();
        private readonly Credentials _requestCredentials;
        private readonly Credentials _adminCredentials;
        private readonly Credentials _pullMonitorCredentials;
        private readonly Endpoint _endpoint;

        public AuthenticationFilterTests()
        {
            _requestCredentials = new Credentials(Guid.NewGuid(), "request");
            _adminCredentials = new Credentials(Guid.NewGuid(), "admin");
            _pullMonitorCredentials = new Credentials(Guid.NewGuid(), "pull");

            SetUpTokenProvider();
            SetUpTimeCoordinator();

            _endpoint = GetTestEndpoint();
        }

        private void SetUpTimeCoordinator()
        {
            _timeCoordinatorMock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
        }

        private void SetUpTokenProvider()
        {
            _tokenProviderMock.Setup(m => m.GetAdminMonitorCredentials()).Returns(_adminCredentials);
            _tokenProviderMock.Setup(m => m.GetPullMonitorCredentials()).Returns(_pullMonitorCredentials);
        }
        

        [Fact]
        public void AuthenticatoionFilter_should_save_principal_into_request_context()
        {
            var authContext = GetAuthContext(_requestCredentials);
            _endpointRegistryMock.Setup(m => m.GetById(_requestCredentials.MonitorId)).Returns(_endpoint);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _tokenProviderMock.Object);

            filter.AuthenticateAsync(authContext, CancellationToken.None);

            Assert.Equal(authContext.Principal.Identity.Name, _endpoint.Identity.Id.ToString());
        }

        [Fact]
        public void AuthenticatoionFilter_should_save_principal_with_administrative_credentials()
        {
            var authContext = GetAuthContext(_adminCredentials);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _tokenProviderMock.Object);

            filter.AuthenticateAsync(authContext, CancellationToken.None);

            Assert.Equal(authContext.Principal.Identity.Name, _adminCredentials.MonitorId.ToString());
        }

        [Fact]
        public void AuthenticatoionFilter_should_save_principal_with_pullmonitor_credentials()
        {
            var authContext = GetAuthContext(_pullMonitorCredentials);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _tokenProviderMock.Object);

            filter.AuthenticateAsync(authContext, CancellationToken.None);

            Assert.Equal(authContext.Principal.Identity.Name, _pullMonitorCredentials.MonitorId.ToString());
        }

        [Fact]
        public void AuthenticatoionFilter_should_not_authenticate_endpoint_with_invalid_credentials()
        {
            var invalidCredentials = new Credentials(Guid.NewGuid(), "invalid");
            var authContext = GetAuthContext(invalidCredentials);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _tokenProviderMock.Object);

            filter.AuthenticateAsync(authContext, CancellationToken.None);

            Assert.Null(authContext.Principal);
        }

        private HttpAuthenticationContext GetAuthContext(Credentials credentials)
        {
            var actionContext = new HttpActionContext(new HttpControllerContext()
            {
                Request = new HttpRequestMessage(),
            }, new ReflectedHttpActionDescriptor());
            var principal = new GenericPrincipal(new GenericIdentity("Basic"), null);
            var context = new HttpAuthenticationContext(actionContext, principal);
            
            context.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", $"{credentials.MonitorId}:{credentials.PrivateToken}".ToBase64String());
            return context;
        }

        private Endpoint GetTestEndpoint()
        {
            return new Endpoint(
                _timeCoordinatorMock.Object,
                new EndpointIdentity(_requestCredentials.MonitorId, "http", "http://endpoint.com", _requestCredentials.PrivateToken.ToSha256Hash()), 
                new EndpointMetadata("endpoint", "group", null));
        }
    }
}
