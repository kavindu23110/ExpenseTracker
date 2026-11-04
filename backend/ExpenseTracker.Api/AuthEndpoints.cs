using System.Security.Claims;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace ExpenseTracker.Api;

/// <summary>Routes for signing in and managing household members.</summary>
public static class AuthEndpoints
{
    public const string OwnerRole = "owner";
    public const string SignInLimit = "sign-in";

    /// <summary>Maps the routes.</summary>
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapGet("/session", async (ClaimsPrincipal user, AuthService s, CancellationToken ct) =>
            new SessionDto(await s.NeedsSetupAsync(ct), CurrentId(user) is { } id ? await s.FindAsync(id, ct) : null));

        auth.MapPost("/setup", async (CredentialsRequest r, HttpContext http, AuthService s, CancellationToken ct) =>
        {
            var member = await s.SetupAsync(r, ct);
            await SignInAsync(http, member);
            return Results.Ok(member);
        }).RequireRateLimiting(SignInLimit);

        auth.MapPost("/login", async (CredentialsRequest r, HttpContext http, AuthService s, CancellationToken ct) =>
        {
            if (await s.SignInAsync(r, ct) is not { } member) return Results.Problem(detail: "That name or password is not right.", statusCode: StatusCodes.Status401Unauthorized);
            await SignInAsync(http, member);
            return Results.Ok(member);
        }).RequireRateLimiting(SignInLimit);

        auth.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.NoContent();
        });

        var owner = new AuthorizeAttribute { Roles = OwnerRole };
        auth.MapGet("/members", (AuthService s, CancellationToken ct) => s.ListAsync(ct)).RequireAuthorization();
        auth.MapPost("/members", async (CredentialsRequest r, AuthService s, CancellationToken ct) => Results.Ok(await s.AddMemberAsync(r, ct))).RequireAuthorization(owner);
        auth.MapPut("/members/{id:int}/whatsapp", async (int id, LinkWhatsAppRequest r, AuthService s, CancellationToken ct) => await s.LinkWhatsAppAsync(id, r.Number, ct) ? Results.NoContent() : Results.NotFound()).RequireAuthorization(owner);
        auth.MapDelete("/members/{id:int}", async (int id, AuthService s, CancellationToken ct) => await s.RemoveMemberAsync(id, ct) ? Results.NoContent() : Results.NotFound()).RequireAuthorization(owner);
    }

    /// <summary>Reads the signed-in member's id.</summary>
    public static int? CurrentId(ClaimsPrincipal user) => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static Task SignInAsync(HttpContext http, MemberDto member)
    {
        List<Claim> claims = [new(ClaimTypes.NameIdentifier, member.Id.ToString()), new(ClaimTypes.Name, member.Name)];
        if (member.IsOwner) claims.Add(new Claim(ClaimTypes.Role, OwnerRole));
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
