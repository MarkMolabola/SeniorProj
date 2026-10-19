using MDRNJ.Api.Controllers;
using MDRNJ.Api.Data;
using MDRNJ.Api.Hubs;
using MDRNJ.Api.Middleware;
using MDRNJ.Api.Services;
using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers & API ──────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── AutoMapper ─────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// ── Entity Framework / SQL Server ───────────────────
// Local dev uses LocalDB (appsettings.Development.json); Azure supplies
// ConnectionStrings__DefaultConnection via App Service settings.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // Azure SQL serverless auto-pauses when idle and takes up to a minute to resume:
        // keep retrying long enough to ride that out
        sql => sql.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

// ── SignalR ────────────────────────────────────────
builder.Services.AddSignalR();

// ── Authentication (JWT) ───────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 characters. Set it in appsettings.Development.json locally or as the Jwt__Key app setting in Azure.");

var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };

        // Allow JWT via query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

// ── Google sign-in (only when Google:ClientId is configured) ──
var googleClientId = builder.Configuration["Google:ClientId"];
if (!string.IsNullOrWhiteSpace(googleClientId))
{
    var appCallback = builder.Configuration["Google:AppCallbackUrl"] ?? AuthController.DefaultAppCallbackUrl;

    authBuilder
        .AddCookie(AuthController.ExternalScheme, options =>
        {
            options.Cookie.Name = "mdrnj.external";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddGoogle(options =>
        {
            options.SignInScheme = AuthController.ExternalScheme;
            options.ClientId = googleClientId;
            options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
            options.ClaimActions.MapJsonKey("email_verified", "email_verified");

            // User cancelled or Google returned an error → send them back to the app.
            options.Events.OnRemoteFailure = context =>
            {
                context.Response.Redirect($"{appCallback}#error=google_sign_in_cancelled");
                context.HandleResponse();
                return Task.CompletedTask;
            };
        });
}

// ── Email (Azure Communication Services, or console logging in dev) ──
if (!string.IsNullOrWhiteSpace(builder.Configuration["Email:ConnectionString"]))
    builder.Services.AddSingleton<IEmailService, AcsEmailService>();
else
    builder.Services.AddSingleton<IEmailService, DevEmailService>();

// ── Challenge proof storage (Azure Blob Storage, or a local folder in dev) ──
var storageConnection = builder.Configuration["Storage:ConnectionString"];
if (!string.IsNullOrWhiteSpace(storageConnection))
    builder.Services.AddSingleton<IProofStorage>(_ =>
        new BlobProofStorage(storageConnection, builder.Configuration["Storage:ProofContainer"] ?? "proofs"));
else
    builder.Services.AddSingleton<IProofStorage>(sp => new LocalProofStorage(
        Path.Combine(builder.Environment.ContentRootPath, "App_Data", "proofs"),
        sp.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>()));

// App Service terminates HTTPS at its proxy; trust X-Forwarded-Proto so the
// Google redirect URI is built as https://.../signin-google.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ── App Services ────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IGoalProgressService, GoalProgressService>();
builder.Services.AddScoped<IGearService, GearService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDungeonService, DungeonService>();
builder.Services.AddScoped<IModerationService, ModerationService>();
builder.Services.AddSingleton<INotifier, Notifier>();
builder.Services.AddSingleton<CharacterClassifierService>();

// ── CORS (allows MAUI app to call the API) ─────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiApp", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Load heroes.jsonc now, so a typo in the balance file stops startup with a clear message
_ = MDRNJ.Shared.Game.HeroAvatars.All;

// ── Seed database ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AchievementSeeder.SeedAsync(db);
}

// ── HTTP Pipeline ────────────────────────────────────
app.UseForwardedHeaders();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowMauiApp");
app.UseAuthentication();
app.UseAuthorization();

// ── Route Mapping ────────────────────────────────────
app.MapControllers();
app.MapHub<LeaderboardHub>("/hubs/leaderboard");
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
