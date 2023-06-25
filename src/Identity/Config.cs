using Duende.IdentityServer.Models;
using IdentityModel;

namespace DotNetFlix.Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("videos-api", "Videos API"),
            new ApiScope("subscriptions-api", "Subscriptions API"),
            new ApiScope("dotnetflix-api", "DotNetFlix API")
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new ApiResource[]
        {
            new ApiResource("VideosApi", "Videos API")
            {
                Scopes = { "videos-api", JwtClaimTypes.Email, "profile" },

                UserClaims =
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.Profile,
                    JwtClaimTypes.GivenName,
                    JwtClaimTypes.FamilyName
                }
            },
            
            new ApiResource("SubscriptionsApi", "Subscriptions API")
            {
                Scopes = { "subscriptions-api", JwtClaimTypes.Email, "profile" },

                UserClaims =
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.Profile,
                    JwtClaimTypes.GivenName,
                    JwtClaimTypes.FamilyName
                }
            },

            new ApiResource("DotnetflixApi", "DotNetFlix API")
            {
                Scopes = { "dotnetflix-api", JwtClaimTypes.Email, "profile" },

                UserClaims =
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.Profile,
                    JwtClaimTypes.GivenName,
                    JwtClaimTypes.FamilyName
                }
            }
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "scope1" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "dotnetflix-api" }
            },

            // Blazor UI client
            new Client
            {
                ClientName = "DotNetFlix Web UI",
                ClientId = "dotnetflix-web-ui",
                RequirePkce = true,
                RequireClientSecret = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,
                AllowedGrantTypes = { "authorization_code" },
                AllowOfflineAccess = true,
                AllowedScopes = {
                "openid",
                "profile",
                "email",
                "dotnetflix-api"
                },
                RedirectUris = { "https://localhost:7009/authentication/login-callback" },
                AllowedCorsOrigins = { "https://localhost:7009" }
            },

            // Videos service client
            new Client
            {
                ClientId = "videos.client",
                ClientName = "Videos Service Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("709CA69A-506A-48D5-84C1-57B01105E660".Sha256()) },

                AllowedScopes = { "videos-api" }
            },

            // Subscriptions service client
            new Client
            {
                ClientId = "subscriptions.client",
                ClientName = "Subscriptions Service Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("11145535-2CE4-4B89-9C94-45DD8EA994E5".Sha256()) },

                AllowedScopes = { "subscriptions-api" }
            },
        };
}
