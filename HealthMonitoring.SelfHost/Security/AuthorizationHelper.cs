using System;
using System.Security.Authentication;
using System.Web.Http.Controllers;
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
            if (modifiable?.Identity.PrivateToken == null ||
                modifiable.Identity.PrivateToken == model?.PrivateToken?.ToSha256Hash())
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
    }
}
