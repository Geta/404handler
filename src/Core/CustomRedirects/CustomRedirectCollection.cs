using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.Data;

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

        public CustomRedirect Find(Uri urlNotFound)
        {
            // Handle absolute addresses first
            var url = urlNotFound.AbsoluteUri;
            var foundRedirect = FindInternal(url);

            // Common case
            if (foundRedirect == null)
            {
                url = urlNotFound.PathAndQuery;
                foundRedirect = FindInternal(url);
            }

            // Handle legacy databases with encoded values
            if (foundRedirect == null)
            {
                url = HttpUtility.HtmlEncode(url);
                foundRedirect = FindInternal(url);
            }

            if (foundRedirect == null)
            {
                url = urlNotFound.AbsoluteUri;
                foundRedirect = FindInProviders(url);
            }

            return foundRedirect;
        }

        public void Add(CustomRedirect customRedirect)
        {
            // Add to quick look up table too
            _quickLookupTable.Add(customRedirect.OldUrl, customRedirect);
        }

        private CustomRedirect FindInternal(string url)
        {
            if (_quickLookupTable.TryGetValue(url, out var redirect))
            {
                return redirect;
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
            using (var enumerator = _quickLookupTable.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    var oldUrl = enumerator.Current.Key;
                    // See if this "old" url (the one that cannot be found) starts with one
                    if (oldUrl != null && url.StartsWith(oldUrl, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var cr = _quickLookupTable[oldUrl];
                        if (cr.State == (int)DataStoreHandler.State.Ignored)
                        {
                            return null;
                        }
                        if (cr.WildCardSkipAppend)
                        {
                            // We'll redirect without appending the 404 url
                            return cr;
                        }

                        if (!UrlIsOldUrlsSubSegment(url, oldUrl))
                        {
                            return null;
                        }

                        // We need to append the 404 to the end of the
                        // new one. Make a copy of the redir object as we
                        // are changing it.
                        var redirCopy = new CustomRedirect(cr);
                        redirCopy.NewUrl = redirCopy.NewUrl + url.Substring(oldUrl.Length);
                        return redirCopy;
                    }
                }
            return null;
        }

        private static bool UrlIsOldUrlsSubSegment(string url, string oldUrl)
        {
            string RemoveQueryString(string u)
            {
                var i = u.IndexOf("?", StringComparison.Ordinal);
                return i < 0 ? u : u.Substring(0, i);

            }

            var urlWithoutQuery = RemoveQueryString(url);
            var isSameUrl = urlWithoutQuery.Equals(oldUrl, StringComparison.OrdinalIgnoreCase);
            var isPartOfOldUrl = urlWithoutQuery.Substring(oldUrl.Length).StartsWith("/");
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
