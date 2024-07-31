using basetemplate_csharp.Data;
using Microsoft.EntityFrameworkCore;
using basetemplate_csharp.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configuración explícita del logging
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

// Cargar variables de entorno desde .env solo en desarrollo
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// Add services to the container.

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        .Replace("${DB_HOST}", Environment.GetEnvironmentVariable("DB_HOST"))
        .Replace("${DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT"))
        .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME"))
        .Replace("${DB_USERNAME}", Environment.GetEnvironmentVariable("DB_USERNAME"))
        .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD"));
    options.UseNpgsql(connectionString) ;
});

// Configure Redis
var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
var redisUsername = Environment.GetEnvironmentVariable("REDIS_USERNAME") ?? "default";
var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "auth";

var redisConfigurationOptions = new ConfigurationOptions
{
    EndPoints = { $"{redisHost}:{redisPort}" },
    User = redisUsername,
    Password = redisPassword
};

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfigurationOptions));
builder.Services.AddSingleton<RedisService>();

// Add PubSubSubscriberService as a hosted service
builder.Services.AddHostedService<PubSubSubscriberService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Verificar la conexión a Redis al iniciar
using (var scope = app.Services.CreateScope())
{
    var redisService = scope.ServiceProvider.GetRequiredService<RedisService>();
    if (!Task.Run(() => redisService.PingAsync(CancellationToken.None)).Result)
    {
        throw new Exception("Unable to connect to Redis.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

