namespace IdentityService.Api.Common.Extensions;

public static class HttpRequestHostExtensions
{
    public static string GetOriginalHost(this HttpRequest request)
    {
        var forwardedHost = request.Headers["X-Forwarded-Host"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwardedHost))
        {
            var firstHost = forwardedHost.Split(',')[0].Trim();

            if (!string.IsNullOrWhiteSpace(firstHost))
            {
                return NormalizeHost(firstHost);
            }
        }

        return NormalizeHost(request.Host.Host);
    }

    private static string NormalizeHost(string host)
    {
        var normalized = host.Trim().ToLowerInvariant();

        var colonIndex = normalized.IndexOf(':');
        if (colonIndex >= 0)
        {
            normalized = normalized[..colonIndex];
        }

        return normalized;
    }
}