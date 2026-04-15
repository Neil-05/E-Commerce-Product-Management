namespace CatalogService.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Items[CorrelationIdMiddleware.HeaderKey]?.ToString()
                ?? context.Request.Headers[CorrelationIdMiddleware.HeaderKey].ToString()
                ?? "-";

            _logger.LogInformation("[Correlation:{cid}] Request: {method} {url}", correlationId, context.Request.Method, context.Request.Path);

            await _next(context);

            _logger.LogInformation("[Correlation:{cid}] Response: {statusCode}", correlationId, context.Response.StatusCode);
        }
    }
}
