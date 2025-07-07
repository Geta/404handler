// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    /// <summary>
    /// A collection of custom urls
    /// </summary>
    public class CustomRedirectCollection : IEnumerable<CustomRedirect>
    {
        private readonly IConfiguration _configuration;

        public CustomRedirectCollection()
            : this(Configuration.Configuration.Instance)
        {
        }

        public CustomRedirectCollection(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Hashtable for quick lookup of old urls
        /// </summary>
        private readonly Dictionary<string, CustomRedirect> _quickLookupTable = new Dictionary<string, CustomRedirect>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Cache of URLs sorted ZA for look up of partially matched URLs
        /// </summary>
        private KeyValuePair<string, CustomRedirect>[] _redirectsZACache;

        public CustomRedirect Find(Uri urlNotFound)
        {
            var foundRedirect = FindWithOptionalDecoding(urlNotFound);

            if (foundRedirect != null) return foundRedirect;

            var url = urlNotFound.AbsoluteUri;
            var decodedAbsoluteUrl = HttpUtility.UrlDecode(url);

            if (string.Compare(decodedAbsoluteUrl, url, StringComparison.OrdinalIgnoreCase) != 0)
            {
                foundRedirect = FindWithOptionalDecoding(urlNotFound, false);
            }

            if (foundRedirect != null) return foundRedirect;

            // Handle legacy databases with encoded values
            url = HttpUtility.HtmlEncode(urlNotFound.PathAndQuery);
            foundRedirect = FindInternal(url);

            // Handle providers
            if (foundRedirect != null) return foundRedirect;
            foundRedirect = FindInProviders(urlNotFound.AbsoluteUri);

            return foundRedirect;
        }

        private CustomRedirect FindWithOptionalDecoding(Uri urlNotFound, bool urlDecode = true)
        {
            var absoluteUrl = urlDecode ? HttpUtility.UrlDecode(urlNotFound.AbsoluteUri) : urlNotFound.AbsoluteUri;
            var foundRedirect = FindInternal(absoluteUrl);

            if (foundRedirect != null) return foundRedirect;

            var pathAndQuery = urlDecode ? HttpUtility.UrlDecode(urlNotFound.PathAndQuery) : urlNotFound.PathAndQuery;
            foundRedirect = FindInternal(pathAndQuery);

            return foundRedirect;
        }

        public void Add(CustomRedirect customRedirect)
        {
            if (_quickLookupTable.ContainsKey(customRedirect.OldUrl))
            {
                _quickLookupTable[customRedirect.OldUrl] = customRedirect;
                return;
            }

            // Add to quick look up table too
            _quickLookupTable.Add(customRedirect.OldUrl, customRedirect);

            // clean cache
            _redirectsZACache = null;
        }

        private CustomRedirect FindInternal(string url)
        {
            if (_quickLookupTable.TryGetValue(url, out var redirect))
            {
                return redirect;
            }

            // working with local copy to avoid multi-threading issues
            var redirectsZA = _redirectsZACache;
            if (redirectsZA == null)
            {
                redirectsZA = _quickLookupTable.OrderByDescending(x => x.Key, StringComparer.OrdinalIgnoreCase).ToArray();
                _redirectsZACache = redirectsZA;
            }

            // No exact match could be done, so we'll check if the 404 url
            // starts with one of the urls we're matching against. This
            // will be kind of a wild card match (even though we only check
            // for the start of the url
            // Example: http://www.mysite.com/news/mynews.html is not found
            // We have defined an "<old>/news</old>" entry in the config
            // file. We will get a match on the /news part of /news/myne...
            // Depending on the skip wild card append setting, we will either
            // redirect using the <new> url as is, or we'll append the 404
            // url to the <new> url.
            foreach (var redirectPair in redirectsZA)
            {
                var oldUrl = redirectPair.Key;
                // See if this "old" url (the one that cannot be found) starts with one
                if (!string.IsNullOrWhiteSpace(oldUrl) && url.StartsWith(oldUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    var cr = redirectPair.Value;
                    if (cr.State == (int)RedirectState.Ignored)
                    {
                        return null;
                    }
                    if (cr.WildCardSkipAppend)
                    {
                        // We'll redirect without appending the 404 url
                        return cr;
                    }

                    if (UrlIsOldUrlsSubSegment(url, oldUrl))
                    {
                        return CreateSubSegmentRedirect(url, cr, oldUrl);
                    }
                }
            }

            return null;
        }

        private static CustomRedirect CreateSubSegmentRedirect(string url, CustomRedirect cr, string oldUrl)
        {
            var redirCopy = new CustomRedirect(cr);

            // splits new URL to query and base
            var newUrl = redirCopy.NewUrl ?? "";
            var newUrlParts = newUrl.Split(new[] { '?' }, 2);
            var newUrlBase = newUrlParts[0];
            var newUrlQuery = newUrlParts.Length > 1 ? "?" + newUrlParts[1] : "";

            // splits the incoming request URL to query and base
            // takes a segment of base in case oldUrl is only a part of it
            var urlBaseSegment = url.Substring(oldUrl.Length);
            var urlParts = urlBaseSegment.Split(new[] { '?' }, 2);
            var urlBase = urlParts[0];
            var urlQuery = urlParts.Length > 1 ? "?" + urlParts[1] : "";

            var appendSegment = (!string.IsNullOrEmpty(urlBase) ? $"/{urlBase}" : "").Replace("//", "/");
            // If the new URL has a query string, we use it.
            // Otherwise, we preserve the query string from the original request.
            var query = !string.IsNullOrEmpty(newUrlQuery) ? newUrlQuery : urlQuery;

            redirCopy.NewUrl = $"{newUrlBase}{appendSegment}{query}";
            return redirCopy;
        }

        private static bool UrlIsOldUrlsSubSegment(string url, string oldUrl)
        {
            string RemoveQueryString(string u)
            {
                var i = u.IndexOf("?", StringComparison.Ordinal);
                return i < 0 ? u : u.Substring(0, i);
            }

            var normalizedUrlWithoutQuery = RemoveQueryString(url).TrimEnd('/');
            var normalizedOldUrl = RemoveQueryString(oldUrl).TrimEnd('/');
            var isSameUrl = normalizedUrlWithoutQuery.Equals(normalizedOldUrl, StringComparison.OrdinalIgnoreCase);
            var isPartOfOldUrl = normalizedUrlWithoutQuery.Substring(normalizedOldUrl.Length).StartsWith("/");
            return isSameUrl || isPartOfOldUrl;
        }

        private CustomRedirect FindInProviders(string oldUrl)
        {
            // If no exact or wildcard match is found, try to parse the url through the custom providers
            foreach (var provider in _configuration.Providers)
            {
                var newUrl = provider.RewriteUrl(oldUrl);
                if (newUrl != null) return new CustomRedirect(oldUrl, newUrl);
            }
            return null;
        }

        public IEnumerator<CustomRedirect> GetEnumerator()
        {
            return _quickLookupTable.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
