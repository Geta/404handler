using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using EPiServer.Framework;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing.Segments;

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    public static class NotFoundPageUtil
    {
        /// <summary>
        /// Gets the content for the 404 page from the language files.
        /// </summary>
        /// <returns></returns>
        public static PageContent Get404PageLanguageResourceContent()
        {
            return new PageContent();
        }

        /// <summary>
        /// Gets the URL that was not found.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static string GetUrlNotFound(HttpRequestBase request)
        {
            string urlNotFound = null;
            var query = request.ServerVariables["QUERY_STRING"];
            if (query != null && query.StartsWith("4"))
            {
                var url = query.Split(';')[1];
                urlNotFound = HttpUtility.UrlDecode(url);
            }
            if (urlNotFound == null)
            {
                if (query != null
                    && query.StartsWith("aspxerrorpath=")
                    && request.Url != null)
                {
                    var parts = query.Split('=');
                    urlNotFound = request.Url.GetLeftPart(UriPartial.Authority) + HttpUtility.UrlDecode(parts[1]);
                }
            }
            return urlNotFound;
        }

        /// <summary>
        /// The refering url
        /// </summary>
        public static string GetReferer(HttpRequestBase request)
        {
            var referer = request.ServerVariables["HTTP_REFERER"];
            if (referer != null)
            {
                // Strip away host name in front, if local redirect
                var hostUrl = SiteDefinition.Current.SiteUrl.ToString();
                if (referer.StartsWith(hostUrl))
                {
                    referer = referer.Remove(0, hostUrl.Length);
                }
            }
            else
            {
                referer = ""; // Can't have null
            }
            return referer;
        }

        public static void HandleOnLoad(Page page, Uri urlNotFound, string referer)
        {
            var statusCode = GetStatusCode(new HttpRequestWrapper(page.Request));
            page.Response.StatusCode = statusCode;
            page.Response.Status = GetStatus(statusCode);

            SetCurrentLanguage(urlNotFound.PathAndQuery);
        }

        public static int GetStatusCode(HttpRequestBase request)
        {
            var code = 404;
            var queryString = GetQueryString(request);

            if (string.IsNullOrEmpty(queryString)) return code;

            var regex = new Regex(@"(?:[0-9]{3}\;)");
            var match = regex.Match(queryString);
            if (match.Success)
            {
                var queryStrings = queryString.Split(';');

                if (queryStrings.Length <= 0) return code;

                if (int.TryParse(queryStrings[0], out code)) return code;
            }
            return code;
        }

        public static string GetQueryString(HttpRequestBase request)
        {
            return HttpUtility.UrlDecode(request.QueryString.ToString());
        }

        public static void SetCurrentLanguage(string url)
        {
            url = url.Substring(1);

            if (!url.Contains("/")) return;

            var languageSegment = url.Substring(0, url.IndexOf('/'));

            if (string.IsNullOrEmpty(languageSegment)) return;

            var languageMatcher = ServiceLocator.Current.GetInstance<ILanguageSegmentMatcher>();
            languageMatcher.TryGetLanguageId(languageSegment, out var languageId);
            if (languageId != null)
            {
                SetContextLanguage(new CultureInfo(languageId));
            }
        }

        private static void SetContextLanguage(CultureInfo culture)
        {
            ContextCache.Current["EPiServer:ContentLanguage"] = culture;
        }

        public static void SetCurrentLanguage(HttpContextBase context)
        {
            var urlNotFound = new Uri($"{SiteDefinition.Current.SiteUrl}{GetUrlNotFound(context.Request)}");
            SetCurrentLanguage(urlNotFound.ToString());
        }

        public static string GetStatus(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    return  "404 File not found";
                case 410:
                    return "410 Deleted";
            }
            return string.Empty;
        }
    }
}
