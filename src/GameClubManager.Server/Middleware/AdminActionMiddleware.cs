using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GameClubManager.Server.Middleware
{
    public class AdminActionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminActionMiddleware> _logger;

        public AdminActionMiddleware(RequestDelegate next, ILogger<AdminActionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Проверяем наличие заголовка X-Admin-Action
            if (context.Request.Headers.TryGetValue("X-Admin-Action", out var adminActionValues) &&
                adminActionValues.FirstOrDefault() == "true")
            {
                _logger.LogInformation("Обнаружен заголовок X-Admin-Action в запросе {Path}", context.Request.Path);
                
                // Проверяем, что запрос касается уведомлений
                if (context.Request.Path.StartsWithSegments("/api/notifications"))
                {
                    var identity = context.User.Identity as ClaimsIdentity;
                    if (identity == null || !identity.IsAuthenticated)
                    {
                        // Создаем новую ClaimsIdentity для действий администратора
                        var adminIdentity = new ClaimsIdentity("AdminAction");
                        adminIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                        
                        // Создаем новый ClaimsPrincipal с новой ClaimsIdentity
                        var adminPrincipal = new ClaimsPrincipal(adminIdentity);
                        context.User = adminPrincipal;
                        
                        _logger.LogInformation("Создан временный администратор для выполнения действия {Method} {Path}", 
                            context.Request.Method, context.Request.Path);
                    }
                }
            }

            // Вызываем следующее middleware в цепочке
            await _next(context);
        }
    }

    public static class AdminActionMiddlewareExtensions
    {
        public static IApplicationBuilder UseAdminActionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdminActionMiddleware>();
        }
    }
} 