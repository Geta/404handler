using System;

using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound.Tests.Base
{
    public static class Extensions
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

        public static string Find(this CustomRedirectCollection collection, string url)
        {
            return collection.Find(url.ToUri())?.NewUrl;
        }
    }
}