using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Azure.Messaging.ServiceBus;
using DotQueue;
using Manager.Constants;
using Manager.Endpoints;
using Manager.Hubs;
using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.QueueMessages;
using Manager.Models.Users;
using Manager.Models.Meetings;
using Manager.Services;
using Manager.Services.Clients.Accessor;
using Manager.Services.Clients.Engine;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Manager;
using Manager.Services.Avatars;
using Manager.Services.Avatars.Models;
using Manager.Services.Clients.Accessor.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("Ai"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<CorsSettings>(
    builder.Configuration.GetSection("Cors"));

builder.Services
  .AddOptions<MeetingOptions>()
  .Bind(builder.Configuration.GetSection(MeetingOptions.SectionName));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = CompressionDefaults.CompressedMimeTypes;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        IdentityModelEventSource.ShowPII = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(AuthSettings.ClockSkewBuffer),

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            NameClaimType = AuthSettings.UserIdClaimType,
            RoleClaimType = AuthSettings.RoleClaimType
        };
    });
builder.Services.AddAuthorization(options =>
{
    // Admin can do anything
    options.AddPolicy(PolicyNames.AdminOnly, p =>
        p.RequireRole(Role.Admin.ToString()));

    // Admin or Teacher (student list/create/update/delete)
    options.AddPolicy(PolicyNames.AdminOrTeacher, p =>
        p.RequireRole(Role.Admin.ToString(), Role.Teacher.ToString()));

    // Exactly Teacher
    options.AddPolicy(PolicyNames.TeacherOnly, p =>
        p.RequireRole(Role.Teacher.ToString()));

    // Admin or Student, for set interests
    options.AddPolicy(PolicyNames.AdminOrStudent, p =>
        p.RequireRole(Role.Admin.ToString(), Role.Student.ToString()));

    // Any authenticated role (handy for groups)
    options.AddPolicy(PolicyNames.AdminOrTeacherOrStudent, p =>
        p.RequireRole(Role.Admin.ToString(), Role.Teacher.ToString(), Role.Student.ToString()));
});

// ---- Services ----
builder.Services.AddControllers();

builder.Services.AddControllers().AddDapr();
var signalROptions = builder.Configuration.GetSection("SignalR");
var keepAlive = signalROptions.GetValue("KeepAliveSeconds", 15);
var clientTimeout = signalROptions.GetValue("ClientTimeoutSeconds", 30);
var signalRBuilder = builder.Services.AddSignalR(o =>
{
    o.KeepAliveInterval = TimeSpan.FromSeconds(keepAlive);
    o.ClientTimeoutInterval = TimeSpan.FromSeconds(clientTimeout);
}
);

var signalRConnString = builder.Configuration["SignalR:ConnectionString"];
if (!string.IsNullOrEmpty(signalRConnString))
{
    signalRBuilder.AddAzureSignalR(signalRConnString);
}

var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>();

if (corsSettings is null || corsSettings.AllowedOrigins is null || corsSettings.AllowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors settings are missing or invalid. Please check appsettings.json.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // Required for sending/receiving cookies
            .WithExposedHeaders("ETag");
    });
});

builder.Services.AddScoped<IAccessorClient, AccessorClient>();
builder.Services.AddScoped<IUsersAccessorClient, UsersAccessorClient>();
builder.Services.AddScoped<IGameAccessorClient, GameAccessorClient>();
builder.Services.AddScoped<IMeetingAccessorClient, MeetingAccessorClient>();
builder.Services.AddScoped<IAchievementAccessorClient, AchievementAccessorClient>();
builder.Services.AddScoped<IEngineClient, EngineClient>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOnlinePresenceService, OnlinePresenceService>();

builder.Services
  .AddOptions<AvatarsOptions>()
  .Bind(builder.Configuration.GetSection(AvatarsOptions.SectionName));

builder.Services.AddSingleton<IAvatarStorageService, AzureBlobAvatarStorageService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSingleton(_ =>
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddQueue<Message, ManagerQueueHandler>(
    QueueNames.ManagerCallbackQueue,
    settings =>
    {
        settings.MaxConcurrentCalls = 5;
        settings.PrefetchCount = 10;
        settings.ProcessingDelayMs = 200;
        settings.MaxRetryAttempts = 3;
        settings.RetryDelaySeconds = 2;
    });
builder.Services.AddSessionQueue<SessionQueueMessage, ManagerSessionQueueHandler>(
    QueueNames.ManagerCallbackSessionQueue,
    settings =>
    {
        settings.MaxConcurrentCalls = 5;
        settings.PrefetchCount = 10;
        settings.ProcessingDelayMs = 200;
        settings.MaxRetryAttempts = 3;
        settings.RetryDelaySeconds = 2;
    });

// This is required for the Scalar UI to have an option to setup an authentication token
builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    }
);
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Trust Kubernetes internal network (example: 10.244.0.0/16)
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.244.0.0"), 16));
    // Optionally allow loopback for local dev
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
});

var refreshRateLimitSettings = builder.Configuration
    .GetSection("RateLimiting:RefreshToken")
    .Get<RefreshTokenRateLimitSettings>();

builder.Services.Configure<RefreshTokenRateLimitSettings>(
    builder.Configuration.GetSection("RateLimiting:RefreshToken"));

if (refreshRateLimitSettings != null)
{
    // Rate Limiting for Refresh Token
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = refreshRateLimitSettings.RejectionStatusCode;
        _ = options.AddPolicy(AuthSettings.RefreshTokenPolicyName, context =>
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? context.Connection.RemoteIpAddress?.ToString()
                 ?? AuthSettings.UnknownIpFallback;

            return RateLimitPartition.Get(ip, _ =>
                new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                {
                    PermitLimit = refreshRateLimitSettings.PermitLimit,
                    Window = refreshRateLimitSettings.WindowMinutes,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = refreshRateLimitSettings.QueueLimit
                }));
        });
    });
}

var app = builder.Build();

var forwardedHeaderOptions = app.Services.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
app.UseForwardedHeaders(forwardedHeaderOptions);
app.UseCors("Frontend");
app.UseResponseCompression();
app.UseCloudEvents();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapSubscribeHandler();
app.MapAiEndpoints();
app.MapAuthEndpoints();
app.MapTasksEndpoints();
app.MapUsersEndpoints();
app.MapGamesEndpoints();
app.MapHub<NotificationHub>("/NotificationHub");
app.MapMediaEndpoints();
app.MapWordCardsEndpoints();
app.MapClassesEndpoints();
app.MapMeetingsEndpoints();
app.MapGameConfigEndpoints();
app.MapAchievementEndpoints();

app.MapStatsPing();
if (env.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Manager API";
        options.Theme = ScalarTheme.BluePlanet;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.ShowSidebar = true;
        options.PersistentAuthentication = true;
        // here we can setup a default token
        //options.AddPreferredSecuritySchemes("Bearer")
        // .AddHttpAuthentication("Bearer", auth =>
        // {
        //     auth.Token = "Some Auth Token...";
        // });
    });
}

app.Run();
