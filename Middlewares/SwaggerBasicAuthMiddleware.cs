using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Middlewares
{
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration config;

        public SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            this.next = next;
            this.config = config;
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

                    if (IsAuthorized(config, username, password))
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
