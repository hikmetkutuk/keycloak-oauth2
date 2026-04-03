using System.Security.Claims;
using KeycloakOAuthApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsEnvironment("Testing"))
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(exceptionHandler =>
    {
        exceptionHandler.Run(context => Results.Problem().ExecuteAsync(context));
    });
}

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
    return claimsPrincipal.Claims
        .GroupBy(claim => claim.Type)
        .ToDictionary(
            group => group.Key,
            group => group.Select(claim => claim.Value).ToArray());
}).RequireAuthorization();

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok", timestamp = DateTimeOffset.UtcNow }))
    .AllowAnonymous();

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}
