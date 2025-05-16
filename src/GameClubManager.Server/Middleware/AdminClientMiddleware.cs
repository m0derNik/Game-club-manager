using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GameClubManager.Server.Middleware
{
    public class AdminClientMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminClientMiddleware> _logger;

        public AdminClientMiddleware(RequestDelegate next, ILogger<AdminClientMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Проверяем наличие заголовка X-Admin-Client
            if (context.Request.Headers.TryGetValue("X-Admin-Client", out var adminClientValues) &&
                adminClientValues.FirstOrDefault() == "true")
            {
                var identity = context.User.Identity as ClaimsIdentity;
                
                // Проверяем, есть ли у пользователя роль Admin
                var hasAdminRole = context.User.IsInRole("Admin");
                
                // Добавляем роль Admin, если её ещё нет
                if (identity != null && !hasAdminRole)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                    _logger.LogInformation("Добавлена роль Admin пользователю через AdminClientMiddleware");
                }
                else if (identity == null)
                {
                    // Создаем новую ClaimsIdentity с ролью Admin, если пользователь не аутентифицирован
                    var newIdentity = new ClaimsIdentity("AdminClient");
                    newIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                    
                    // Создаем новый ClaimsPrincipal с новой ClaimsIdentity
                    var newPrincipal = new ClaimsPrincipal(newIdentity);
                    context.User = newPrincipal;
                    
                    _logger.LogInformation("Создан новый пользователь с ролью Admin через AdminClientMiddleware");
                }
            }

            // Вызываем следующее middleware в цепочке
            await _next(context);
        }
    }

    public static class AdminClientMiddlewareExtensions
    {
        public static IApplicationBuilder UseAdminClientMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AdminClientMiddleware>();
        }
    }
} 