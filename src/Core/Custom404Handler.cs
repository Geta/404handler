using System;
using System.Web;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;

namespace BVNetwork.NotFound.Core
{
    public class Custom404Handler
    {

        public static bool HandleRequest(string referer, Uri urlNotFound, out string newUrl)
        {
            // Try to match the requested url my matching it
            // to the static list of custom redirects
            CustomRedirectHandler fnfHandler = CustomRedirectHandler.Current;
            CustomRedirect redirect = fnfHandler.CustomRedirects.Find(urlNotFound);
            string pathAndQuery = HttpUtility.HtmlEncode(urlNotFound.PathAndQuery);
            newUrl = null;
            if (redirect == null)
            {
                // Not found, lets try without the host
                redirect = fnfHandler.CustomRedirects.FindInProviders(urlNotFound.AbsoluteUri);
            }

            if (redirect != null)
            {

                if (redirect.State.Equals((int)DataStoreHandler.GetState.Saved))
                {
                    // Found it, however, we need to make sure we're not running in an
                    // infinite loop. The new url must not be the referrer to this page
                    if (string.Compare(redirect.NewUrl, pathAndQuery, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        newUrl = redirect.NewUrl;
                        return true;
                        // Referer is not new url, means we can safely redirect
                        ////TODO: ending the response too
                        //_log.Info(String.Format("404 Custom Redirect: To: '{0}' (from: '{1}')", redirect.NewUrl, pathAndQuery));

                        ////Changed so that search engines update their statistics and links correctly.
                        //page.Response.Clear();
                        //page.Response.StatusCode = 301;
                        //page.Response.StatusDescription = "Moved Permanently";
                        //page.Response.RedirectLocation = redirect.NewUrl;
                        //page.Response.End();
                    }
                }
            }
            else
            {
                // log request to database - if logging is turned on.
                //if (Settings.EnableLogging)
                //{
                Logger.LogRequest(pathAndQuery, referer);

                // }
            }

            return false;
        }
    }
}