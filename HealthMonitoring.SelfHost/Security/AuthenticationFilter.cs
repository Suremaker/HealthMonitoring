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
        public ICredentialsProvider CredentialsProvider{ get; set; }
        public IEndpointRegistry EndpointRegistry { get; set; }

        public bool AllowMultiple { get; }
        private const string _passwordKey = "Password";

        public AuthenticationFilter(IEndpointRegistry endpointRegistry, ICredentialsProvider credentialsProvider)
        {
            CredentialsProvider = credentialsProvider;
            EndpointRegistry = endpointRegistry;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            IPrincipal principal = null;
            GenericIdentity identity;

            var credentials = context.ParseAuthorizationHeader();
            var adminCred = CredentialsProvider.GetAdminCredentials();
            var pullCred = CredentialsProvider.GetMonitorCredentials();

            if (credentials == null)
                return Task.FromResult(0);

            if (credentials.Equals(adminCred))
            {
                identity = new GenericIdentity(credentials.Id.ToString());
                principal = new GenericPrincipal(identity, new[] { SecurityRole.Admin.ToString() });

            }else if (credentials.Equals(pullCred))
            {
                identity = new GenericIdentity(pullCred.Id.ToString());
                principal = new GenericPrincipal(identity, new[] {SecurityRole.Monitor.ToString()});
            }
            else
            {
                string encryptedPassword = credentials.Password.ToSha256Hash();
                var endpoint = EndpointRegistry.GetById(credentials.Id);

                if (endpoint?.Password == encryptedPassword)
                {
                    context.Request.Properties[_passwordKey] = encryptedPassword;
                    identity = new GenericIdentity(credentials.Id.ToString());
                    principal = new GenericPrincipal(identity, null);
                }
            }

            context.Principal = principal;

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
