using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KeycloakOAuthApi.Extensions;

public static class ServiceCollectionExtensions
{
    private const string ServiceName = "Keycloak.OAuth.Api";
    private const string SwaggerScheme = "Keycloak";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddAuthorization();
        services.AddKeycloakAuthentication(configuration);
        services.AddSwaggerDocumentation(configuration);
        services.AddOpenTelemetryServices();

        return services;
    }

    public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services)
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

        return services;
    }

    private static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
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

        return services;
    }

    private static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = configuration["Keycloak:Issuer"]
            ?? throw new InvalidOperationException("Missing configuration value: Keycloak:Issuer");

        var metadataAddress = configuration["Keycloak:MetadataAddress"]
            ?? throw new InvalidOperationException("Missing configuration value: Keycloak:MetadataAddress");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.MetadataAddress = metadataAddress;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = false,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles"
                };
            });

        return services;
    }
}
