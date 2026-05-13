namespace WebApiShop.MiddleWare
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                _logger.LogError(ex, ex.Message);

                var errorResponse = new
                {
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    innerInner = ex.InnerException?.InnerException?.Message,
                    stackTrace = ex.StackTrace
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
