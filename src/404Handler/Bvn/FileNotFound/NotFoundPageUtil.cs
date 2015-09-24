using System;
using System.Web;
using BVNetwork.FileNotFound.Redirects;
using BVNetwork.FileNotFound.Content;
using BVNetwork.Bvn.FileNotFound.Logging;
using BVNetwork.Bvn.FileNotFound.Upgrade;

namespace BVNetwork.FileNotFound
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
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("Trying to handle 404 for \"{0}\" (Referrer: \"{1}\")", urlNotFound, referer);
            }
            // Try to match the requested url my matching it
            // to the static list of custom redirects
            CustomRedirectHandler fnfHandler = CustomRedirectHandler.Current;
            CustomRedirect redirect = fnfHandler.CustomRedirects.Find(urlNotFound);
            string pathAndQuery = HttpUtility.HtmlEncode(urlNotFound.PathAndQuery);

            if (redirect == null)
            {
                // Not found, lets look in the custom providers
                redirect = fnfHandler.CustomRedirects.FindInProviders(HttpUtility.HtmlEncode(urlNotFound.AbsoluteUri));
            }

            if (redirect != null)
            {
                if (redirect.State.Equals((int)BVNetwork.FileNotFound.DataStore.DataStoreHandler.GetState.Saved))
                {
                    // Found it, however, we need to make sure we're not running in an
                    // infinite loop. The new url must not be the referrer to this page
                    if (string.Compare(redirect.NewUrl, pathAndQuery, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        // Referer is not new url, means we can safely redirect
                        // ending the response too
                        _log.Info(String.Format("404 Custom Redirect: To: '{0}' (from: '{1}')", redirect.NewUrl, pathAndQuery));

                        //Changed so that search engines update their statistics and links correctly.
                        page.Response.Clear(); 
                        page.Response.StatusCode = 301;
                        page.Response.StatusDescription = "Moved Permanently";
                        page.Response.RedirectLocation = redirect.NewUrl;
                        page.Response.End();
                        return;
                    }
                }
            }
            else
            {
                // log request to database - if logging is turned on.
                if (Configuration.Configuration.Logging == Configuration.LoggerMode.On && Upgrader.Valid)
                {
                    Logger.LogRequest(pathAndQuery, referer);
                }
            }
            // We need to signal that this is indeed a 404 error
            page.Response.TrySkipIisCustomErrors = true;
            page.Response.StatusCode = 404;
            page.Response.Status = "404 File not found";
        }
    }
}
