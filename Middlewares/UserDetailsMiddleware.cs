using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace InvoiceApp.Middlewares
{
    public class UserDetailsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserDetailsMiddleware> _logger;
        private readonly IConfiguration _config;

        public UserDetailsMiddleware(RequestDelegate next, ILogger<UserDetailsMiddleware> logger, IConfiguration config)
        {
            _next = next;
            _logger = logger;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Assuming this middleware runs after authentication middleware
            if (context.User.Identity.IsAuthenticated && !context.Request.Path.StartsWithSegments("/swagger"))
            {
                var userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userName = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                context.Items["UserId"] = userId;
                context.Items["UserName"] = userName;
                context.Items["Email"] = email;
            }

            await _next(context);
        }


        //public async Task InvokeAsync(HttpContext context)
        //{
        //    var token = context.Request.Headers["Authorization"].ToString()?.Split("Bearer ")?.LastOrDefault();

        //    var validIssuer = _config["JwtTokenSettings:ValidIssuer"];
        //    var validAudience = _config["JwtTokenSettings:ValidAudience"];
        //    var symmetricSecurityKey = _config["JwtTokenSettings:SymmetricSecurityKey"];

        //    var tokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricSecurityKey)),
        //        ValidateIssuer = true,
        //        ValidIssuer = validIssuer,
        //        ValidateAudience = true,
        //        ValidAudience = validAudience,
        //        ValidateLifetime = true, 
        //        ClockSkew = TimeSpan.Zero 
        //    };


        //    if (!string.IsNullOrEmpty(token) && !context.Request.Path.StartsWithSegments("/swagger") && !token.Contains("Basic"))
        //    {
        //        var handler = new JwtSecurityTokenHandler();
        //        try
        //        {
        //            // Validate the token
        //            var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

        //            // Ensure the token is a JWT token
        //            if (validatedToken is JwtSecurityToken jwtToken)
        //            {
        //                var userId = principal.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        //                var userName = principal.Claims.First(claim => claim.Type == ClaimTypes.Name)?.Value;
        //                var email = principal.Claims.First(claim => claim.Type == ClaimTypes.Email)?.Value;

        //                context.Items["UserId"] = userId;
        //                context.Items["UserName"] = userName;
        //                context.Items["Email"] = email;
        //                context.Items["AccessToken"] = token;
        //            }
        //        }
        //        catch (SecurityTokenException)
        //        {
        //            // Token validation failed
        //            // Log the exception or handle the validation failure
        //            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //            await context.Response.WriteAsync("Invalid token");
        //            return;
        //        }
        //    }

        //    await _next(context);
        //}

    }
}
