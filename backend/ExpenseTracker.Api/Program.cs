using ExpenseTracker.Api;
using ExpenseTracker.Application;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BillOptions>(builder.Configuration.GetSection(BillOptions.Section));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.Cookie.Name = "expense-session";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    // A private hostname is plain HTTP in the browser, inside the encrypted WARP connection.
    if (builder.Configuration.GetValue("Auth:SecureCookie", !builder.Environment.IsDevelopment())) o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.ExpireTimeSpan = TimeSpan.FromDays(14);
    o.SlidingExpiration = true;
    o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
    // A removed member's session ends straight away.
    o.Events.OnValidatePrincipal = async c =>
    {
        var auth = c.HttpContext.RequestServices.GetRequiredService<AuthService>();
        if (c.Principal is null || AuthEndpoints.CurrentId(c.Principal) is not { } id || await auth.FindAsync(id, c.HttpContext.RequestAborted) is null) c.RejectPrincipal();
    };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter(AuthEndpoints.SignInLimit, l => { l.PermitLimit = 10; l.Window = TimeSpan.FromMinutes(1); });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseInitialiser>().RunAsync();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapAuthEndpoints();
app.MapBillEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
