using System;

namespace BVNetwork.NotFound.Tests.Base
{
    public static class Uris
    {
        public static Uri ToUri(this string url)
        {
            return ToUri(url, "http://example.com");
        }

        public static Uri ToUri(this string url, string fallbackBaseUrl)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri)) return uri;
            return new Uri(new Uri(fallbackBaseUrl), url);
        }
    }
}