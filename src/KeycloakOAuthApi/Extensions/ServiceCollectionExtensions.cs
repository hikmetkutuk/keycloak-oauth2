using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using KeycloakOAuthApi.Infrastructure;
using KeycloakOAuthApi.Security;

namespace KeycloakOAuthApi.Extensions;

public static class ServiceCollectionExtensions
{
    private const string ServiceName = "Keycloak.OAuth.Api";
    private const string SwaggerScheme = "Keycloak";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiServices(IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddProblemDetails();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                {
                    policy.RequireRole(Roles.Admin);
                });
            });

            services.AddSingleton<IClaimsTransformation, KeycloakRealmRoleClaimsTransformation>();
            services.AddSingleton<ICommerceStore, InMemoryCommerceStore>();

            services.AddKeycloakAuthentication(configuration);
            services.AddSwaggerDocumentation(configuration);
            services.AddOpenTelemetryServices();

            return services;
        }

        private void AddOpenTelemetryServices()
        {
            services
                .AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(ServiceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();
                });
        }

        private void AddSwaggerDocumentation(IConfiguration configuration)
        {
            var authorizationUrl = configuration["Keycloak:AuthorizationUrl"]
                                   ?? throw new InvalidOperationException("Missing configuration value: Keycloak:AuthorizationUrl");

            var tokenUrl = configuration["Keycloak:TokenUrl"]
                           ?? throw new InvalidOperationException("Missing configuration value: Keycloak:TokenUrl");

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Keycloak OAuth API",
                    Version = "v1",
                    Description = "Sample .NET 10 API secured with Keycloak and OAuth 2.0."
                });

                options.CustomSchemaIds(type => type.FullName?.Replace('+', '-'));

                options.AddSecurityDefinition(SwaggerScheme, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(authorizationUrl),
                            TokenUrl = new Uri(tokenUrl),
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "OpenID Connect scope",
                                ["profile"] = "User profile scope"
                            }
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = SwaggerScheme
                            }
                        },
                        ["openid", "profile"]
                    }
                });
            });
        }

        private void AddKeycloakAuthentication(IConfiguration configuration)
        {
            var audience = configuration["Authentication:Audience"]
                           ?? throw new InvalidOperationException("Missing configuration value: Authentication:Audience");

            var issuer = configuration["Authentication:Issuer"]
                         ?? throw new InvalidOperationException("Missing configuration value: Authentication:Issuer");

            var metadataAddress = configuration["Authentication:MetadataAddress"]
                                  ?? throw new InvalidOperationException("Missing configuration value: Authentication:MetadataAddress");

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = audience;
                    options.MetadataAddress = metadataAddress;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        NameClaimType = "preferred_username",
                        RoleClaimType = "roles"
                    };
                });
        }
    }
}
