using System.Net;

namespace MDRNJfitness.Helpers
{
    /// <summary>
    /// Retries requests the server couldn't handle yet: 502/503/504, e.g. while Azure
    /// wakes the app or resumes the paused database. Waits a little longer each time.
    /// </summary>
    public class TransientRetryHandler : DelegatingHandler
    {
        private static readonly TimeSpan[] Delays = { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) };

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Buffer the body so the request can be sent again
            var body = request.Content is null ? null : await request.Content.ReadAsByteArrayAsync(cancellationToken);

            var response = await base.SendAsync(request, cancellationToken);
            foreach (var delay in Delays)
            {
                if (!IsTransient(response.StatusCode)) break;

                response.Dispose();
                await Task.Delay(delay, cancellationToken);
                using var retry = Clone(request, body);
                response = await base.SendAsync(retry, cancellationToken);
            }
            return response;
        }

        private static bool IsTransient(HttpStatusCode status) =>
            status is HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;

        internal static HttpRequestMessage Clone(HttpRequestMessage original, byte[]? body)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri) { Version = original.Version };
            foreach (var header in original.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            if (body is not null)
            {
                clone.Content = new ByteArrayContent(body);
                foreach (var header in original.Content!.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            return clone;
        }
    }
}
