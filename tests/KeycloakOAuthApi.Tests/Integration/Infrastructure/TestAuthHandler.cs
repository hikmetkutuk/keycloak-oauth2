using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeycloakOAuthApi.Tests.Integration.Infrastructure;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestAuth";
    public const string SubHeaderName = "X-Test-Sub";
    public const string UsernameHeaderName = "X-Test-Username";
    public const string RolesHeaderName = "X-Test-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(SubHeaderName, out var subValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var sub = subValues.ToString();
        if (string.IsNullOrWhiteSpace(sub))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing test subject."));
        }

        var username = Request.Headers.TryGetValue(UsernameHeaderName, out var usernameValues)
            ? usernameValues.ToString()
            : sub;

        var roleValues = Request.Headers.TryGetValue(RolesHeaderName, out var rolesHeader)
            ? rolesHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];

        var claims = new List<Claim>
        {
            new("sub", sub),
            new("preferred_username", username),
            new(ClaimTypes.Name, username)
        };

        claims.AddRange(roleValues.Select(role => new Claim("roles", role)));

        var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, "roles");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
