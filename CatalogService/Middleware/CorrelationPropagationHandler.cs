using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CatalogService.Middleware
{
    public class CorrelationPropagationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<CorrelationPropagationHandler> _logger;
        public const string HeaderKey = "X-Correlation-ID";

        public CorrelationPropagationHandler(IHttpContextAccessor httpContext, ILogger<CorrelationPropagationHandler> logger)
        {
            _httpContext = httpContext;
            _logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var cid = _httpContext?.HttpContext?.Items[CorrelationIdMiddleware.HeaderKey]?.ToString()
                    ?? _httpContext?.HttpContext?.Request?.Headers[CorrelationIdMiddleware.HeaderKey].ToString();

                if (!string.IsNullOrWhiteSpace(cid))
                {
                    if (!request.Headers.Contains(HeaderKey))
                    {
                        request.Headers.Add(HeaderKey, cid);
                        _logger.LogDebug("[Correlation:{cid}] Propagated to outgoing request {method} {url}", cid, request.Method, request.RequestUri);
                    }
                }
            }
            catch
            {
                // best-effort
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
