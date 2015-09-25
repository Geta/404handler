using System;
using System.Web;

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    public static class NotFoundPageUtil
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        public static Uri GetUrlNotFound(System.Web.UI.Page page)
        {
            Uri uriNotFound = null;
            string urlNotFound = "";
            string query = page.Request.ServerVariables["QUERY_STRING"];
            if ((query != null) && query.StartsWith("404;"))
            {
                string url = query.Split(';')[1];
                if (!Uri.TryCreate(url, UriKind.Absolute, out uriNotFound))
                {
                    uriNotFound = new Uri(HttpUtility.UrlDecode(url));
                }

            }
            if (uriNotFound == null)
            {
                if (query.StartsWith("aspxerrorpath="))
                {
                    string[] parts = query.Split('=');
                    uriNotFound = new Uri(page.Request.Url.GetLeftPart(UriPartial.Authority) + HttpUtility.UrlDecode(parts[1]));
                }
            }
            return uriNotFound;
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
                string hostUrl = EPiServer.Configuration.Settings.Instance.SiteUrl.ToString();
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
