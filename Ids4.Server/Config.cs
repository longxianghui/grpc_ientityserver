using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.Test;

namespace Ids4.Server
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };
        }
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "Demo API")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };
        }
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
                {
                    new Client
                    {
                        ClientId = "client",
                        ClientSecrets = { new Secret("secret".Sha256()) },

                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        AllowedScopes = { "api" },
                    },
                };
        }
    }
}
