using System.Security.Claims;
using KeycloakOAuthApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(
            app.Configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Missing configuration value: Keycloak:ClientId"));
        options.OAuthUsePkce();
        options.OAuthScopes("openid", "profile");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("users/me", (ClaimsPrincipal claimsPrincipal) =>
{
    return claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
}).RequireAuthorization();

await app.RunAsync();
