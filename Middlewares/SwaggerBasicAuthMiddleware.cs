using System.Net;
using System.Text;
using InvoiceApp.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Middlewares
{
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SwaggerBasicAuthMiddleware> _logger;

        public SwaggerBasicAuthMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<SwaggerBasicAuthMiddleware> logger)
        {
            this.next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                    var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                    var username = decodedUsernamePassword.Split(':', 2)[0];
                    var password = decodedUsernamePassword.Split(':', 2)[1];

                    if (await IsAuthorized(username, password))
                    {
                        await next.Invoke(context);
                        return;
                    }
                }

                context.Response.Headers["WWW-Authenticate"] = "Basic";

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await next.Invoke(context);
            }
        }

        private async Task<bool> IsAuthorized(string username, string password)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var swaggerCredentialsService = scopedServices.GetRequiredService<ISwaggerCredentialsService>();

                _logger.LogInformation("Starting to validate credentials.");
                bool result = false;

                try
                {
                    result = await swaggerCredentialsService.ValidateCredentialsAsync(username, password);

                    _logger.LogInformation("User signed it successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while signin user.");
                }

                return result;
            }
        }
             
        public static bool IsAuthorized(IConfiguration config, string username, string password)
        {
            var _userName = config["SettingConstant:SwaggerSetting_UserName"];
            var _password = config["SettingConstant:SwaggerSetting_Password"];

            return username.Equals(_userName, StringComparison.InvariantCultureIgnoreCase) && password.Equals(_password);
        }
    }

    public class SettingConstant
    {
        public static string SwaggerSetting_UserName { set ; get; }
        public static string SwaggerSetting_Password { set ; get; }
    }
}
