using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace KeycloakOAuthApi.Security;

public sealed class KeycloakRealmRoleClaimsTransformation(
    ILogger<KeycloakRealmRoleClaimsTransformation> logger) : IClaimsTransformation
{
    private const string RealmAccessClaimType = "realm_access";
    private const string ResourceAccessClaimType = "resource_access";
    private const string JsonRolesProperty = "roles";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
        {
            return Task.FromResult(principal);
        }

        var existingRoles = identity.FindAll(identity.RoleClaimType)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AddRealmRoles(principal.FindFirst(RealmAccessClaimType)?.Value, identity, existingRoles);
        AddResourceRoles(principal.FindFirst(ResourceAccessClaimType)?.Value, identity, existingRoles);

        return Task.FromResult(principal);
    }

    private void AddRealmRoles(string? claimValue, ClaimsIdentity identity, HashSet<string> existingRoles)
    {
        if (string.IsNullOrWhiteSpace(claimValue))
        {
            return;
        }

        try
        {
            using var json = JsonDocument.Parse(claimValue);
            if (!json.RootElement.TryGetProperty(JsonRolesProperty, out var roles) || roles.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var roleElement in roles.EnumerateArray())
            {
                AddRoleClaim(roleElement.GetString(), identity, existingRoles);
            }
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception,
                "Malformed Keycloak claim encountered during role mapping. ClaimType: {ClaimType}",
                RealmAccessClaimType);
        }
    }

    private void AddResourceRoles(string? claimValue, ClaimsIdentity identity, HashSet<string> existingRoles)
    {
        if (string.IsNullOrWhiteSpace(claimValue))
        {
            return;
        }

        try
        {
            using var json = JsonDocument.Parse(claimValue);
            if (json.RootElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (var clientElement in json.RootElement.EnumerateObject())
            {
                if (!clientElement.Value.TryGetProperty(JsonRolesProperty, out var roles) || roles.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var roleElement in roles.EnumerateArray())
                {
                    AddRoleClaim(roleElement.GetString(), identity, existingRoles);
                }
            }
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception,
                "Malformed Keycloak claim encountered during role mapping. ClaimType: {ClaimType}",
                ResourceAccessClaimType);
        }
    }

    private static void AddRoleClaim(string? role, ClaimsIdentity identity, HashSet<string> existingRoles)
    {
        if (string.IsNullOrWhiteSpace(role) || !existingRoles.Add(role))
        {
            return;
        }

        identity.AddClaim(new Claim(identity.RoleClaimType, role));
    }
}
