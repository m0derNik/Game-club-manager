using GameClubManager.Server.Data;
using GameClubManager.Server.Middleware;
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
    
    // Добавляем поддержку авторизации в Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
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

// Настройка аутентификации JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

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

// Добавляем аутентификацию в конвейер
app.UseAuthentication();

// Добавляем middleware для проверки админского клиента ПЕРЕД авторизацией
app.UseAdminClientMiddleware();

// Добавляем отладочное логирование
app.Use(async (context, next) => 
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var path = context.Request.Path.ToString();
    
    // Определяем пути, которые не нужно логировать для неаутентифицированных пользователей
    var excludedPaths = new[] { 
        "/api/remotecontrol", 
        "/api/games/public",
        "/api/tariffs",
        "/api/food",
        "/api/computers"
    };
    
    // Проверяем, нужно ли логировать этот запрос
    bool shouldLog = !excludedPaths.Any(excludedPath => 
        path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase));
    
    if (context.User.Identity?.IsAuthenticated == true)
    {
        // Всегда логируем запросы аутентифицированных пользователей
        logger.LogInformation("Запрос к {Path}: Пользователь аутентифицирован, имя: {Name}", 
            path, context.User.Identity.Name);
            
        var claims = context.User.Claims.ToList();
        foreach (var claim in claims)
        {
            logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
        }
    }
    else if (shouldLog)
    {
        // Логируем запросы неаутентифицированных пользователей только если путь не в списке исключений
        logger.LogInformation("Запрос к {Path}: Пользователь не аутентифицирован", path);
    }
    
    await next();
});

app.UseAuthorization();

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