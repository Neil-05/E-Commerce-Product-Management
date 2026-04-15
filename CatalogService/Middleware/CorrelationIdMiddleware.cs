using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Microsoft.Extensions.Logging;

namespace CatalogService.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public const string HeaderKey = "X-Correlation-ID";

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderKey].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.Items[HeaderKey] = correlationId;
            context.Response.Headers[HeaderKey] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogInformation("[REQUEST] {method} {url}",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);

                _logger.LogInformation("[RESPONSE] {status}",
                    context.Response.StatusCode);
            }
        }
    }
}