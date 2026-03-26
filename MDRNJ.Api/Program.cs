using MDRNJ.Api.Data;
using MDRNJ.Api.Hubs;
using MDRNJ.Api.Middleware;
using MDRNJ.Api.Services;
using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers & API ──────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── AutoMapper ─────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

// ── Entity Framework / SQLite ───────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── SignalR ────────────────────────────────────────
builder.Services.AddSignalR();

// ── Authentication (JWT) ───────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
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

// ── Seed database ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AchievementSeeder.SeedAsync(db);
}

// ── HTTP Pipeline ────────────────────────────────────
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

app.Run();
