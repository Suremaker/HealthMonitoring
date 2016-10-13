using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using HealthMonitoring.Management.Core;
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Entities;

namespace HealthMonitoring.SelfHost.Security
{
    public static class AuthorizationHelper 
    {
        public static void AuthorizeRegistration(this HttpRequestContext context, EndpointRegistration model,
            Endpoint modifiable, params SecurityRole[] roles)
        {
            if (modifiable?.PrivateToken == null ||
                modifiable.PrivateToken == model?.PrivateToken?.ToSha256Hash())
                return;

            Authorize(context, modifiable.Identity.Id, roles);
        }

        public static void Authorize(this HttpRequestContext context, Guid endpointId, params SecurityRole[] roles)
        {
            if (context.Principal == null)
                throw new AuthenticationException();

            bool authorized = context.IsSelfAuthorized(endpointId);

            authorized |= IsInRoles(context, roles);

            if(!authorized)
                throw new UnauthorizedAccessException();
        }

        public static void Authorize(this HttpRequestContext context, params SecurityRole[] roles)
        {
            if (!IsInRoles(context, roles))
                throw new UnauthorizedAccessException();
        }

        public static Credentials ParseAuthorizationHeader(this HttpAuthenticationContext context)
        {
            return ParseCredentialsFromRequestHeader(context.Request);
        }

        public static Credentials ParseAuthorizationHeader(this ExceptionHandlerContext context)
        {
            return ParseCredentialsFromRequestHeader(context.Request);
        }

        private static bool IsSelfAuthorized(this HttpRequestContext context, Guid endpointId)
        {
            return endpointId.ToString() == context.Principal.Identity.Name;
        }

        private static bool IsInRoles(HttpRequestContext context, params SecurityRole[] roles)
        {
            bool inRoles = false;

            foreach (var role in roles)
            {
                inRoles |= context.Principal.IsInRole(role.ToString());
            }
            return inRoles;
        }

        private static Credentials ParseCredentialsFromRequestHeader(HttpRequestMessage message)
        {
            string authHeader = null;
            string schema = "Basic";
            var auth = message.Headers.Authorization;

            if (auth != null && string.Equals(auth.Scheme, schema, StringComparison.OrdinalIgnoreCase))
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
    }
}
