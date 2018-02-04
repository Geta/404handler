using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using BVNetwork.NotFound.Configuration;
using BVNetwork.NotFound.Core.Data;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    /// <summary>
    /// A collection of custom urls
    /// </summary>
    public class CustomRedirectCollection: CollectionBase
    {
        /// <summary>
        /// Hashtable for quick lookup of old urls
        /// </summary>
        private readonly Dictionary<string, CustomRedirect> _quickLookupTable = new Dictionary<string, CustomRedirect>();

        /*[CanBeNull]*/
        private string GetLookupKey(/*[NotNull]*/ CustomRedirect customRedirect)
        {
            if (customRedirect == null) throw new ArgumentNullException(nameof(customRedirect));
            return GetLookupKey(customRedirect.OldUrl);
        }

        /*[CanBeNull]*/
        private string GetLookupKey(/*[CanBeNull]*/ string url)
        {
            return UrlStandardizer.Standardize(url);
        }

        private void AddLookup(/*[NotNull]*/ CustomRedirect customRedirect)
        {
            if (customRedirect == null) throw new ArgumentNullException(nameof(customRedirect));

            _quickLookupTable[GetLookupKey(customRedirect)] = customRedirect;
        }

        private void RemoveLookup(/*[NotNull]*/ CustomRedirect customRedirect)
        {
            _quickLookupTable.Remove(GetLookupKey(customRedirect));
        }

        /*[CanBeNull]*/
        private CustomRedirect TryLookup(/*[NotNull]*/ string url)
        {
            _quickLookupTable.TryGetValue(GetLookupKey(url), out var foundRedirect);
            return foundRedirect;
        }

        public int Add(CustomRedirect customRedirect)
        {
            // Add to quick look up table too
            AddLookup(customRedirect);
            return List.Add(customRedirect);
        }

        public int IndexOf(CustomRedirect customRedirect)
        {
            for(var i = 0; i < List.Count; i++)
            {
                if (this[i] == customRedirect) return i; // Found
            }
            return -1;
        }

        public void Insert(int index, CustomRedirect customRedirect)
        {
            AddLookup(customRedirect);
            List.Insert(index, customRedirect);
        }

        public void Remove(CustomRedirect customRedirect)
        {
            RemoveLookup(customRedirect);
            List.Remove(customRedirect);
        }

        /*[CanBeNull]*/
        public CustomRedirect Find(/*[NotNull]*/ Uri urlNotFound)
        {
            if (urlNotFound == null) throw new ArgumentNullException(nameof(urlNotFound));

            var urlsToSearch = new[]
            {
                // Handle absolute addresses first
                urlNotFound.AbsoluteUri,
                // and its protocol invariant version
                RemoveProtocol(urlNotFound.AbsoluteUri),
                // then try it without query
                RemoveQuery(urlNotFound.AbsoluteUri),
                // and the protocol invariant version
                RemoveProtocol(RemoveQuery(urlNotFound.AbsoluteUri)),
                // common case 
                urlNotFound.PathAndQuery,
                // and the same without query
                RemoveQuery(urlNotFound.PathAndQuery),
                // Handle legacy databases with encoded values
                HttpUtility.HtmlEncode(urlNotFound.PathAndQuery),
                // and the same without query
                HttpUtility.HtmlEncode(RemoveQuery(urlNotFound.PathAndQuery))
            };

            var foundRedirect = TryFind(urlsToSearch);
            var result = PostProcessRedirect(urlNotFound, foundRedirect);
            return result;
        }

        /*[NotNull]*/
        string RemoveProtocol(/*[NotNull]*/ string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            const string http = "http:";
            if (url.StartsWith(http))
                return url.Substring(http.Length);

            const string https = "https:";
            if (url.StartsWith(https))
                return url.Substring(https.Length);

            return url;
        }

        /*[NotNull]*/
        string RemoveQuery(/*[NotNull]*/ string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            var queryStartsAt = url.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);

            var result = queryStartsAt > -1 ? url.Substring(0, queryStartsAt) : url;
            return result;
        }

        /*[CanBeNull]*/
        private CustomRedirect TryFind(/*[CanBeNull]*/ params string[] urls)
        {
            foreach (var url in urls.Distinct(StringComparer.InvariantCultureIgnoreCase))
            {
                var foundRedirect = FindInternal(url);
                if (foundRedirect != null)
                    return foundRedirect;
            }

            return null;
        }

        /*[CanBeNull]*/
        private CustomRedirect PostProcessRedirect(/*[NotNull]*/Uri urlNotFound, /*[CanBeNull]*/ CustomRedirect redirect)
        {
            if (urlNotFound == null) throw new ArgumentNullException(nameof(urlNotFound));
            if (redirect == null)
                return null;

            if (redirect.ExactMatch)
            {
                if (!string.IsNullOrEmpty(urlNotFound.Query))
                {
                    if (!redirect.SkipQueryString)
                    {
                        var newUrlParts = redirect.NewUrl.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                        var newUrlQuery = newUrlParts.Length > 1 ? newUrlParts[1] : null;

                        var originalQueryParsed = HttpUtility.ParseQueryString(urlNotFound.Query);
                        var targetQueryParsed = newUrlQuery != null
                            ? HttpUtility.ParseQueryString(newUrlQuery)
                            : new NameValueCollection();

                        var appendQuery = Merge(targetQueryParsed, originalQueryParsed);
                        var newUrl = redirect.NewUrl;

                        redirect = new CustomRedirect(redirect)
                        {
                            NewUrl = newUrl.Contains("?") ? $"{newUrl}&{appendQuery}" : $"{newUrl}?{appendQuery}"
                        };
                    }
                }
            }
            else
            {
                if (!redirect.WildCardSkipAppend)
                {
                    // We need to append the 404 to the end of the
                    // new one. Make a copy of the redir object as we
                    // are changing it.
                    var url = urlNotFound.ToString();
                    var redirCopy = new CustomRedirect(redirect);
                    var urlFromRule = UrlStandardizer.Standardize(redirect.OldUrl);

                    var append = IsAbsoluteUrl(urlFromRule) ? url.Substring(urlFromRule.Length) : urlNotFound.PathAndQuery.Substring(urlFromRule.Length);

                    if (append != string.Empty)
                    {
                        redirCopy.NewUrl = UrlStandardizer.Standardize(redirCopy.NewUrl + append);
                        return redirCopy;
                    }
                }
            }

            return redirect;
        }

        private bool IsAbsoluteUrl(/*[NotNull]*/ string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            return url.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)
                   || url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                   || url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Merges query parameters from the http request with ones from the redirect rule (the target query takes precedence).  
        /// For instance: 
        /// <list type="bullet">
        ///     <item>
        ///         <term>Rule:</term>
        ///         <description>http://mysite/test => http://mysite?a=1&amp;b=2</description>
        ///     </item>
        ///     <item>
        ///         <term>Request:</term>
        ///         <description>http://mysite/test?a=0&amp;x=0</description>
        ///     </item>
        ///     <item>
        ///         <term>Result:</term>
        ///         <description>a=1&amp;b=2&amp;x=0 </description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="targetQuery">The target query as it's specified in the redirect rule</param>
        /// <param name="originalQuery">The original query from http request</param>
        /// <returns>Returns the merged query string parameters that can be appended to the url</returns>
        /*[NotNull]*/
        private string Merge(/*[NotNull]*/ NameValueCollection targetQuery, /*[NotNull]*/ NameValueCollection originalQuery)
        {
            if (targetQuery == null) throw new ArgumentNullException(nameof(targetQuery));
            if (originalQuery == null) throw new ArgumentNullException(nameof(originalQuery));

            // append all keys from the original query that are not present in the target one, and all the target query keys
            var appendQueryArray = originalQuery.AllKeys.Where(x => targetQuery[x] == null)
                .Select(x => $"{HttpUtility.UrlEncode(x)}={HttpUtility.UrlEncode(originalQuery[x])}").ToArray();
            var query = string.Join("&", appendQueryArray);
            return query;
        }

        private CustomRedirect FindInternal(string url)
        {
            var foundRedirect = TryLookup(url);

            if (foundRedirect != null)
            {
                return foundRedirect;
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

            foreach (var pair in _quickLookupTable)
            {
                var redirect = pair.Value;
                if (redirect.ExactMatch)
                    continue; 
                // See if this "old" url (the one that cannot be found) starts with one 
                if (url.StartsWith(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (redirect.State == (int)DataStoreHandler.State.Ignored)
                    {
                        return null;
                    }

                    return redirect;
                }
            }
            return null;
        }


        public CustomRedirect FindInProviders(string oldUrl)
        {
            // If no exact or wildcard match is found, try to parse the url through the custom providers
            var providers = Bvn404HandlerConfiguration.Instance.Bvn404HandlerProviders;
            if (providers == null || providers.Count == 0) return null;

            foreach (Bvn404HandlerProvider provider in Bvn404HandlerConfiguration.Instance.Bvn404HandlerProviders)
            {
                var type = (Type.GetType(provider.Type));
                if (type != null)
                {
                    var handler = (INotFoundHandler)Activator.CreateInstance(type);
                    var newUrl = handler.RewriteUrl(oldUrl);
                    if (newUrl != null) return new CustomRedirect(oldUrl, newUrl);
                }
            }
            return null;
        }

        public bool Contains(string oldUrl)
        {
            return _quickLookupTable.ContainsKey(GetLookupKey(oldUrl));
        }

        public CustomRedirect this[int index]
        {
            get => (CustomRedirect) List[index];
            set => List[index] = value;
        }
    }
}
