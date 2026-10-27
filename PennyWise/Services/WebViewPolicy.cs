namespace PennyWise.Services;

/// <summary>
/// Whitelist for the external preview screen. Only www.cnb.cz and its
/// subdomains over HTTPS are allowed; everything else is cancelled in the
/// Navigating handler and reported with a DisplayAlert.
/// </summary>
public static class WebViewPolicy
{
    public const string StartUrl = "https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/";

    private const string AllowedHost = "cnb.cz";

    /// <summary>Blank documents produced by the WebView itself, before the first load.</summary>
    public static bool IsInternalNavigation(string? url)
        => string.IsNullOrWhiteSpace(url)
           || url.StartsWith("about:blank", StringComparison.OrdinalIgnoreCase)
           || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase);

    public static bool IsAllowed(string? url)
    {
        if (IsInternalNavigation(url))
        {
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // Relative URLs ("/cs/...") stay on the current document.
            return !url!.Contains("://", StringComparison.Ordinal);
        }

        if (uri.Scheme is not ("https" or "http"))
        {
            return false;
        }

        var host = uri.Host;

        return host.Equals(AllowedHost, StringComparison.OrdinalIgnoreCase)
               || host.EndsWith("." + AllowedHost, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Human readable host, used in the blocking alert.</summary>
    public static string DescribeHost(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        return url;
    }
}
