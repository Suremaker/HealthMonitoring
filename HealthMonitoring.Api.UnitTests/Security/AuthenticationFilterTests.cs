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
        private readonly Mock<ICredentialsProvider> _credentialsProviderMock = new Mock<ICredentialsProvider>();
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

            SetUpCredentialsProvider();
            SetUpTimeCoordinator();

            _endpoint = GetTestEndpoint();
        }

        private void SetUpTimeCoordinator()
        {
            _timeCoordinatorMock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
        }

        private void SetUpCredentialsProvider()
        {
            _credentialsProviderMock.Setup(m => m.GetAdminCredentials()).Returns(_adminCredentials);
            _credentialsProviderMock.Setup(m => m.GetMonitorCredentials()).Returns(_pullMonitorCredentials);
        }
        

        [Fact]
        public void AuthenticatoionFilter_should_save_principal_into_request_context()
        {
            var authContext = GetAuthContext(_requestCredentials);
            _endpointRegistryMock.Setup(m => m.GetById(_requestCredentials.Id)).Returns(_endpoint);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _credentialsProviderMock.Object);

            filter.AuthenticateAsync(authContext, CancellationToken.None);

            Assert.Equal(authContext.Principal.Identity.Name, _endpoint.Identity.Id.ToString());
        }

        [Fact]
        public void AuthenticatoionFilter_should_save_principal_with_administrative_credentials()
        {
            var authContext = GetAuthContext(_adminCredentials);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _credentialsProviderMock.Object);

            filter.AuthenticateAsync(authContext, CancellationToken.None);

            Assert.Equal(authContext.Principal.Identity.Name, _adminCredentials.Id.ToString());
        }

        [Fact]
        public void AuthenticatoionFilter_should_save_principal_with_pullmonitor_credentials()
        {
            var authContext = GetAuthContext(_pullMonitorCredentials);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _credentialsProviderMock.Object);

            filter.AuthenticateAsync(authContext, CancellationToken.None);

            Assert.Equal(authContext.Principal.Identity.Name, _pullMonitorCredentials.Id.ToString());
        }

        [Fact]
        public void AuthenticatoionFilter_should_not_authenticate_endpoint_with_invalid_credentials()
        {
            var invalidCredentials = new Credentials(Guid.NewGuid(), "invalid");
            var authContext = GetAuthContext(invalidCredentials);
            var filter = new AuthenticationFilter(_endpointRegistryMock.Object, _credentialsProviderMock.Object);

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
            
            context.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", $"{credentials.Id}:{credentials.Password}".ToBase64String());
            return context;
        }

        private Endpoint GetTestEndpoint()
        {
            return new Endpoint(
                _timeCoordinatorMock.Object,
                new EndpointIdentity(_requestCredentials.Id, "http", "http://endpoint.com"), 
                new EndpointMetadata("endpoint", "group", null),
                _requestCredentials.Password.ToSha256Hash());
        }
    }
}
