using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Config order: base -> env -> local -> env vars -> ocelot base -> ocelot local
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true) // git-ignored
    .AddEnvironmentVariables()
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ocelot.Local.json", optional: true, reloadOnChange: true);     // optional local ocelot overrides

// Read JWT settings (must match Manager)
var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];
var secret = jwt["Secret"];

if (string.IsNullOrWhiteSpace(secret))
    throw new InvalidOperationException("Jwt:Secret is missing (put it in appsettings.Local.json or environment variables).");

var keyBytes = Encoding.UTF8.GetBytes(secret);

// AuthN
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Auth");
            log.LogInformation("Auth PASSED for {Path}", ctx.HttpContext.Request.Path);
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            var log = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Auth");
            log.LogWarning(ctx.Exception, "Auth FAILED for {Path}", ctx.HttpContext.Request.Path);
            return Task.CompletedTask;
        },
        OnChallenge = ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return ctx.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
        }
    };
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseWebSockets();

app.UseAuthentication();
app.UseAuthorization();

// Acceptance-criteria logs
app.Use(async (ctx, next) =>
{
    await next();
    var log = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Auth");
    var authed = ctx.User?.Identity?.IsAuthenticated == true;
    log.LogInformation("Request {Method} {Path} => {Status} (authenticated={Auth})",
        ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, authed);
});

await app.UseOcelot();
await app.RunAsync();
