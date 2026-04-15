using Microsoft.AspNetCore.Http;
using Serilog.Context;
namespace ApiGateway.Middleware
{
 
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        public const string HeaderKey = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var cid = context.Request.Headers[HeaderKey].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(cid))
                cid = Guid.NewGuid().ToString();

            context.Response.Headers[HeaderKey] = cid;

            using (LogContext.PushProperty("CorrelationId", cid))
            {
                await _next(context);
            }
        }
    }
}
