using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using HealthMonitoring.Management.Core.Registers;
using HealthMonitoring.Security;

namespace HealthMonitoring.SelfHost.Security
{
    public class AuthenticationFilter : IAuthenticationFilter
    {
        public ICredentialsProvider TokenProvider { get; set; }
        public IEndpointRegistry EndpointRegistry { get; set; }

        public bool AllowMultiple { get; }
        private const string _scheme = "Basic";
        private const string _tokenKey = "PrivateToken";

        public AuthenticationFilter(IEndpointRegistry endpointRegistry, ICredentialsProvider tokenProvider)
        {
            TokenProvider = tokenProvider;
            EndpointRegistry = endpointRegistry;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            IPrincipal principal = null;
            GenericIdentity identity;

            var credentials = ParseAuthorizationHeader(context);
            var adminCred = TokenProvider.GetAdminMonitorCredentials();
            var pullCred = TokenProvider.GetPullMonitorCredentials();

            if (credentials == null)
                return Task.FromResult(0);

            if (credentials.Equals(adminCred))
            {
                identity = new GenericIdentity(credentials.MonitorId.ToString());
                principal = new GenericPrincipal(identity, new[] { SecurityRole.AdminMonitor.ToString() });

            }else if (credentials.Equals(pullCred))
            {
                identity = new GenericIdentity(pullCred.MonitorId.ToString());
                principal = new GenericPrincipal(identity, new[] {SecurityRole.PullMonitor.ToString()});
            }
            else
            {
                string encryptedToken = credentials.PrivateToken.ToSha256Hash();
                var endpoint = EndpointRegistry.GetById(credentials.MonitorId);

                if (endpoint?.Identity.PrivateToken == encryptedToken)
                {
                    context.Request.Properties[_tokenKey] = encryptedToken;
                    identity = new GenericIdentity(credentials.MonitorId.ToString());
                    principal = new GenericPrincipal(identity, null);
                }
            }

            context.Principal = principal;

            return Task.FromResult(0);
        }

        protected virtual Credentials ParseAuthorizationHeader(HttpAuthenticationContext context)
        {
            string authHeader = null;
            var auth = context.Request.Headers.Authorization;

            if (auth != null && string.Equals(auth.Scheme, _scheme, StringComparison.OrdinalIgnoreCase))
            {
                authHeader = auth.Parameter;
            }

            if (string.IsNullOrEmpty(authHeader))
            {
                return null;
            }

            authHeader = authHeader.FromBase64String();
            var credentials = authHeader.Split(':');
            Guid id;

            if (credentials.Length != 2 ||
                !Guid.TryParse(credentials[0], out id) || string.IsNullOrEmpty(credentials[1]))
            {
                return null;
            }

            return new Credentials(id, credentials[1]);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
