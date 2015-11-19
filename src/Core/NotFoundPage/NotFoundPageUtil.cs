using System;
using System.Web;
using EPiServer;
using EPiServer.Logging;
using EPiServer.Web;

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
        /// <param name="page">The request page.</param>
        /// <returns></returns>
        public static string GetUrlNotFound(System.Web.UI.Page page)
        {
            string query = page.Request.ServerVariables["QUERY_STRING"];
            if (query != null && query.StartsWith(Custom404Handler.NotFoundParam))
            {
                return Url.Decode(query).Substring(Custom404Handler.NotFoundParam.Length + 2);
            }
            return null;
        }

        /// <summary>
        /// The refering url
        /// </summary>
        public static string GetReferer(System.Web.UI.Page page)
        {
            string referer = page.Request.ServerVariables["HTTP_REFERER"];
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

        public static void HandleOnLoad(System.Web.UI.Page page, Uri urlNotFound, string referer)
        {
            // We need to signal that this is indeed a 404 error
            page.Response.TrySkipIisCustomErrors = true;
            page.Response.StatusCode = 404;
            page.Response.Status = "404 File not found";
        }
    }
}
