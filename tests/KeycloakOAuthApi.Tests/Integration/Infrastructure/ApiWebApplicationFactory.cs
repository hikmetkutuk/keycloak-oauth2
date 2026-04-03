using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KeycloakOAuthApi.Tests.Integration.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Audience"] = "public-client",
                ["Authentication:Issuer"] = "http://localhost:8081/realms/auth",
                ["Authentication:MetadataAddress"] = "http://localhost:8081/realms/auth/.well-known/openid-configuration",
                ["Keycloak:ClientId"] = "public-client",
                ["Keycloak:AuthorizationUrl"] = "http://localhost:8081/realms/auth/protocol/openid-connect/auth",
                ["Keycloak:TokenUrl"] = "http://localhost:8081/realms/auth/protocol/openid-connect/token"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public HttpClient CreateAuthenticatedClient(string sub, string username, params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.SubHeaderName, sub);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UsernameHeaderName, username);

        if (roles.Length > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeaderName, string.Join(",", roles));
        }

        return client;
    }
}
