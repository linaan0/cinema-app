using System.Text;
using CinemaApp.Bookings.Api;
using CinemaApp.Bookings.Api.Hubs;
using CinemaApp.Bookings.Domain.Models;
using CinemaApp.Bookings.Repository;
using CinemaApp.Bookings.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ---- MongoDB ----
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IMongoDatabase>(_ =>
{
    var settings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>()
        ?? throw new InvalidOperationException("MongoDb settings are missing.");
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

// ---- Redis (seat locking) ----
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]
        ?? throw new InvalidOperationException("Redis:ConnectionString is missing.")));

// ---- Repositories ----
builder.Services.AddScoped<IScreeningRepository, ScreeningRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// ---- Application services ----
builder.Services.AddScoped<ISeatLockService, RedisSeatLockService>();
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<ISeatStatusNotifier, SignalRSeatStatusNotifier>();
builder.Services.AddScoped<IBookingService, BookingService>();

// ---- SignalR for live seat-map updates ----
builder.Services.AddSignalR();

// ---- JWT auth (validates tokens issued by the Auth service) ----
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };

        // SignalR can't send Authorization headers on the WebSocket handshake,
        // so it sends the JWT as ?access_token=... instead - accept that here.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR with credentials requires a specific-origin (not wildcard) CORS policy.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SeatAvailabilityHub>("/hubs/seat-availability");
app.MapHealthChecks("/health");

// ---- Seed a demo screening on first run so the app is demoable immediately ----
using (var scope = app.Services.CreateScope())
{
    var screeningRepository = scope.ServiceProvider.GetRequiredService<IScreeningRepository>();
    var existing = await screeningRepository.GetAllAsync();
    if (!existing.Any())
    {
        var seats = new List<Seat>();
        foreach (var row in new[] { "A", "B", "C" })
        {
            for (var number = 1; number <= 6; number++)
            {
                seats.Add(new Seat { SeatId = $"{row}{number}", Row = row, Number = number });
            }
        }

        await screeningRepository.InsertAsync(new Screening
        {
            MovieId = "demo-movie-1",
            HallId = "Hall 1",
            StartTime = DateTime.UtcNow.AddDays(1),
            BasePrice = 6.50m,
            Seats = seats
        });
    }
}

app.Run();
