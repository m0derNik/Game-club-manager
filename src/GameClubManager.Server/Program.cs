using GameClubManager.Server.Data;
using GameClubManager.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Настройка URL для сервера
builder.WebHost.UseUrls("http://localhost:7001", "https://localhost:7000");

// Добавляем сервисы в контейнер
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Game Club Manager API", Version = "v1" });
});

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Настройка базы данных
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация сервисов
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

// Настройка конвейера HTTP-запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseCors();

// Включаем HTTPS в режиме разработки
app.UseHttpsRedirection();

app.MapControllers();

// Создание базы данных при первом запуске
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Начало инициализации базы данных");
        
        // Проверяем существование базы данных
        if (!context.Database.CanConnect())
        {
            logger.LogInformation("База данных не существует, создаем новую");
            await context.Database.EnsureCreatedAsync();
        }
        
        // Инициализируем данные
        await context.InitializeDatabaseAsync();
        
        // Проверяем наличие пользователей
        var userCount = await context.Users.CountAsync();
        logger.LogInformation("База данных успешно создана и инициализирована. Количество пользователей: {UserCount}", userCount);
    }
    catch (Exception ex)
    {
        var errorLogger = services.GetRequiredService<ILogger<Program>>();
        errorLogger.LogError(ex, "Ошибка при создании базы данных");
    }
}

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var appLogger = loggerFactory.CreateLogger<Program>();
appLogger.LogInformation("Сервер запущен на http://localhost:7001");

app.Run(); 