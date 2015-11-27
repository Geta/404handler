using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using EPiServer.Framework;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing.Segments;

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    public static class NotFoundPageUtil
    {
        private static readonly ILogger _log = LogManager.GetLogger(typeof(NotFoundPageUtil));

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
            string query = request.ServerVariables["QUERY_STRING"];
            if ((query != null) && query.StartsWith("4"))
            {
                string url = query.Split(';')[1];
                urlNotFound = HttpUtility.UrlDecode(url);
            }
            if (urlNotFound == null)
            {
                if (query.StartsWith("aspxerrorpath="))
                {
                    string[] parts = query.Split('=');
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
            string referer = request.ServerVariables["HTTP_REFERER"];
            if (referer != null)
            {
                // Strip away host name in front, if local redirect
                string hostUrl = SiteDefinition.Current.SiteUrl.ToString();
                if (referer.StartsWith(hostUrl))
                    referer = referer.Remove(0, hostUrl.Length);
            }
            else
                referer = ""; // Can't have null
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
            int code = 0;
            string queryString = GetQueryString(request);
            if (!string.IsNullOrEmpty(queryString))
            {
                Regex regex = new Regex(@"(?:[0-9]{3}\;)");
                Match match = regex.Match(queryString);
                if (match.Success)
                {
                    string[] queryStrings = queryString.Split(';');
                    if (queryStrings.Length > 0)
                    {
                        if (int.TryParse(queryStrings[0], out code))
                            return code;
                    }
                }
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
            if (url.Contains("/"))
            {
                string languageSegment = url.Substring(0, url.IndexOf('/'));
                if (!string.IsNullOrEmpty(languageSegment))
                {
                    var languageMatcher = ServiceLocator.Current.GetInstance<ILanguageSegmentMatcher>();
                    string languageId;
                    languageMatcher.TryGetLanguageId(languageSegment, out languageId);
                    if (languageId != null)
                        ContextCache.Current["EPiServer:ContentLanguage"] = new CultureInfo(languageId);
                }
            }
        }

        public static string GetStatus(int statusCode)
        {
            string status = "";
            if (statusCode == 410)
                status =  "404 File not found";
            else if (statusCode == 404)
                status = "410 Deleted";
            return status;
        }
    }
}
