using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//builder.Services.AddAutoMapper(typeof(Program));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// ── Controllers & API ──────────────────────────────
//builder.Services.AddControllers();
//builder.Services.AddOpenApi();

//// ── AutoMapper ─────────────────────────────────────
//builder.Services.AddAutoMapper(typeof(Program));

//// ── Entity Framework / Azure SQL ───────────────────
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection")));

//// ── Redis Cache ────────────────────────────────────
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration =
//        builder.Configuration.GetConnectionString("Redis");
//});

//// ── SignalR ────────────────────────────────────────
//builder.Services.AddSignalR();

//// ── Authentication (Azure AD B2C) ──────────────────
//builder.Services.AddAuthentication("Bearer")
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.Authority =
//            builder.Configuration["AzureAdB2C:Authority"];
//        options.Audience =
//            builder.Configuration["AzureAdB2C:ClientId"];
//    });

//// ── App Services (your own services) ───────────────
//builder.Services.AddScoped<IWorkoutService, WorkoutService>();
//builder.Services.AddScoped<IGameService, GameService>();
//builder.Services.AddScoped<ICharacterService, CharacterService>();
//builder.Services.AddScoped<IChallengeService, ChallengeService>();
//builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
//builder.Services.AddScoped<IFriendService, FriendService>();

//// ── CORS (allows MAUI app to call the API) ─────────
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowMauiApp", policy =>
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader());
//});

//var app = builder.Build();

//// ── HTTP Pipeline ───────────────────────────────────
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();
//app.UseCors("AllowMauiApp");
//app.UseAuthentication();
//app.UseAuthorization();

//// ── Route Mapping ───────────────────────────────────
//app.MapControllers();
//app.MapHub<LeaderboardHub>("/hubs/leaderboard");

//app.Run();