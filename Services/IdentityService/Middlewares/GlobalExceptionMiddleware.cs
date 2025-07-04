using System.Net;
using System.Text.Json;
using SharedKernel.Common.Exceptions;

namespace IdentityService.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            httpContext.Response.ContentType = "application/json";
            
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string code = "GlobalExceptionMiddleware: InternalServerError";
            string message = ex.Message;

            if (ex is BaseAppException appException)
            {
                statusCode = appException.StatusCode ?? statusCode;
                code = appException.Code;
                message = appException.Message;
            }
            
            httpContext.Response.StatusCode = statusCode;
            var json = JsonSerializer.Serialize(new
            {
                code,
                message
            });

            await httpContext.Response.WriteAsync(json);
        }
    }
}