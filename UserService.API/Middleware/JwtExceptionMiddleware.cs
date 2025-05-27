using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace UserService.API.Middleware
{
    public class JwtExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtExceptionMiddleware> _logger;

        public JwtExceptionMiddleware(RequestDelegate next, ILogger<JwtExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "JWT token expired at {Path}.", context.Request.Path);
                await HandleJwtExceptionAsync(context, "Token has expired.");
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "JWT token signature invalid at {Path}.", context.Request.Path);
                await HandleJwtExceptionAsync(context, "Token signature is invalid.");
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT token error at {Path}.", context.Request.Path);
                await HandleJwtExceptionAsync(context, "Invalid or expired token.");
            }
        }

        private static async Task HandleJwtExceptionAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            context.Response.Headers["Cache-Control"] = "no-store";

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
