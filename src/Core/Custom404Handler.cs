using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;
using EPiServer.Web;
using log4net;
using IPAddress = System.Net.IPAddress;

namespace BVNetwork.NotFound.Core
{
    public class Custom404Handler
    {
        private const string NotFoundParam = "notfound";

        private static readonly List<string> _ignoredResourceExtensions = new List<string> { "jpg", "gif", "png", "css", "js", "ico", "swf" };

        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                    }
                }
            }
            else
            {
                // log request to database - if logging is turned on.
                if (Configuration.Configuration.Logging == LoggerMode.On)
                {
                    Logger.LogRequest(pathAndQuery, referer);
                }
            }
            return false;
        }

        public static void FileNotFoundHandler(object sender, EventArgs evt)
        {
            // Check if this should be enabled
            if (Configuration.Configuration.FileNotFoundHandlerMode == FileNotFoundMode.Off)
                return;
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("FileNotFoundHandler called");
            }
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                if (_log.IsDebugEnabled)
                    _log.Debug("No HTTPContext, returning");
                return;
            }

            if (context.Response.StatusCode != 404)
                return;
            string query = context.Request.ServerVariables["QUERY_STRING"];
            if ((query != null) && query.StartsWith("404;"))
            {
                return;
            }

            Uri notFoundUri = context.Request.Url;

            // Skip resource files
            if (IsResourceFile(notFoundUri))
                return;

            // If we're only doing this for remote users, we need to test for local host
            if (Configuration.Configuration.FileNotFoundHandlerMode == FileNotFoundMode.RemoteOnly)
            {
                // Determine if we're on localhost
                bool localHost = IsLocalhost();
                if (localHost)
                {
                    if (_log.IsDebugEnabled)
                        _log.Debug("Determined to be localhost, returning");
                    return;
                }
                if (_log.IsDebugEnabled)
                    _log.Debug("Not localhost, handling error");
            }

            // Avoid looping forever
            if (IsInfiniteLoop(context))
                return;

            string newUrl;
            if (HandleRequest(GetReferer(context.Request.UrlReferrer), notFoundUri, out newUrl))
            {
                context.Response.RedirectPermanent(newUrl);
            }
            else
            {
                string url = Get404Url();

                context.Response.Clear();
                context.Response.TrySkipIisCustomErrors = true;
                context.Server.ClearError();

                // do the redirect to the 404 page here
                if (HttpRuntime.UsingIntegratedPipeline)
                {
                    context.Server.TransferRequest(url, true);
                }
                else
                {
                    context.RewritePath(url, false);
                    IHttpHandler httpHandler = new MvcHttpHandler();
                    httpHandler.ProcessRequest(context);
                }
                // return the original status code to the client
                // (this won't work in integrated pipleline mode)
                context.Response.StatusCode = 404;
            }
        }

        /// <summary>
        /// Determines whether the specified not found URI is a resource file
        /// </summary>
        /// <param name="notFoundUri">The not found URI.</param>
        /// <returns>
        /// 	<c>true</c> if it is a resource file; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsResourceFile(Uri notFoundUri)
        {
            string extension = notFoundUri.AbsolutePath;
            int extPos = extension.LastIndexOf('.');
            if (extPos > 0)
            {
                extension = extension.Substring(extPos + 1);
                if (_ignoredResourceExtensions.Contains(extension))
                {
                    // Ignoring 404 rewrite of known resource extension
                    if (_log.IsDebugEnabled)
                        _log.DebugFormat("Ignoring rewrite of '{0}'. '{1}' is a known resource extension", notFoundUri.ToString(),
                                         extension);

                    return true;
                }
            }
            return false;
        }

        private static bool IsInfiniteLoop(HttpContext ctx)
        {
            string requestUrl = ctx.Request.Url.AbsolutePath;
            string fnfPageUrl = Get404Url();
            if (fnfPageUrl.StartsWith("~"))
                fnfPageUrl = fnfPageUrl.Substring(1);
            int posQuery = fnfPageUrl.IndexOf("?");
            if (posQuery > 0)
                fnfPageUrl = fnfPageUrl.Substring(0, posQuery);

            if (string.Compare(requestUrl, fnfPageUrl, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                _log.Info("404 Handler detected an infinite loop to 404 page. Exiting");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the current request is on localhost.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if current request is localhost; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsLocalhost()
        {
            bool localHost = false;
            try
            {
                System.Net.IPAddress address = System.Net.IPAddress.Parse(HttpContext.Current.Request.UserHostAddress);
                Debug.WriteLine("IP Address of user: " + address, "404Handler");

                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                Debug.WriteLine("Host Entry of local computer: " + host.HostName, "404Handler");
                localHost = address.Equals(IPAddress.Loopback) || (Array.IndexOf(host.AddressList, address) >= 0);
            }
            catch
            {
                // localhost is false
            }
            return localHost;
        }

        public static string GetReferer(Uri referer)
        {
            string refererUrl = "";
            if (referer != null)
            {
                refererUrl = referer.AbsolutePath;
                if (!string.IsNullOrEmpty(refererUrl))
                {
                    // Strip away host name in front, if local redirect
                    
                    string hostUrl = SiteDefinition.Current.SiteUrl.ToString();
                    if (refererUrl.StartsWith(hostUrl))
                        refererUrl = refererUrl.Remove(0, hostUrl.Length);
                }
            }
            return refererUrl;
        }

        /// <summary>
        /// Creates a url to the 404 page, containing the aspxerrorpath query
        /// variable with information about the current request url
        /// </summary>
        /// <returns></returns>
        private static string Get404Url()
        {
            string baseUrl = Configuration.Configuration.FileNotFoundHandlerPage;
            string currentUrl = HttpContext.Current.Request.Url.PathAndQuery;
            return String.Format("{0}?{1}={2}", baseUrl, NotFoundParam, HttpContext.Current.Server.UrlEncode(currentUrl));
        }
    }
}