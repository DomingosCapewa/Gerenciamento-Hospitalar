using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Shared.Contracts.Security
{
   
    public class KeycloakRoleClaimsTransformer : IClaimsTransformation
    {
        private readonly string? _clientId;

        public KeycloakRoleClaimsTransformer(IConfiguration configuration)
        {
            _clientId = configuration["Authentication:ClientId"]; 
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
                return Task.FromResult(principal);

            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var realmAccess = principal.FindFirst("realm_access")?.Value;
            if (!string.IsNullOrWhiteSpace(realmAccess))
            {
                try
                {
                    using var doc = JsonDocument.Parse(realmAccess);
                    if (doc.RootElement.TryGetProperty("roles", out var arr))
                    {
                        foreach (var r in arr.EnumerateArray())
                        {
                            var v = r.GetString();
                            if (!string.IsNullOrWhiteSpace(v)) roles.Add(v);
                        }
                    }
                }
                catch { }
            }

            var resourceAccess = principal.FindFirst("resource_access")?.Value;
            if (!string.IsNullOrWhiteSpace(resourceAccess))
            {
                try
                {
                    using var doc = JsonDocument.Parse(resourceAccess);
                    if (!string.IsNullOrWhiteSpace(_clientId) && doc.RootElement.TryGetProperty(_clientId, out var client) && client.TryGetProperty("roles", out var cr))
                    {
                        foreach (var r in cr.EnumerateArray())
                        {
                            var v = r.GetString();
                            if (!string.IsNullOrWhiteSpace(v)) roles.Add(v);
                        }
                    }
                    else
                    {
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (prop.Value.TryGetProperty("roles", out var list))
                            {
                                foreach (var r in list.EnumerateArray())
                                {
                                    var v = r.GetString();
                                    if (!string.IsNullOrWhiteSpace(v)) roles.Add(v);
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            foreach (var role in roles)
            {
            
            if (!principal.IsInRole(role))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    identity.AddClaim(new Claim("roles", role));
                }
            }

            return Task.FromResult(principal);
        }
    }
}
