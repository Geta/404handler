using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;
using EPiServer.Core;
using log4net;
using IPAddress = System.Net.IPAddress;

namespace BVNetwork.NotFound.Core
{
    public class Custom404Handler
    {
        private const string ASPX_ERROR_PARAM = "aspxerrorpath";

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
                // TODO: According to the comment, this is not correc.t
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


        public static void FileNotFoundHandler(object sender, EventArgs evt)
        {


            // Check if this should be enabled
            //TODO: if (Configuration.Configuration.FileNotFoundHandlerMode == FileNotFoundMode.Off)


            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat("FileNotFoundHandler called");
            }


            HttpContext ctx = HttpContext.Current;

            Exception exception;

            if (ctx == null)
            {
                if (_log.IsDebugEnabled)
                    _log.Debug("No HTTPContext, returning");
                return;
            }

            // Get the error
            bool isAspError = false;
            try
            {
                exception = ctx.Server.GetLastError();
                if (exception != null)
                    isAspError = true;
            }
            catch
            {
                if (_log.IsDebugEnabled)
                    _log.Debug("Cannot GetLastError, returning");
                return;
            }

            if (ctx.Response.StatusCode != 404)
                return;
            string query = ctx.Request.ServerVariables["QUERY_STRING"];
            if ((query != null) && query.StartsWith("404;"))
            {
                return;
            }

            Uri notFoundUri = ctx.Request.Url;

            // Skip resource files
            if (IsResourceFile(notFoundUri))
                return;

            // If we're only doing this for remote users, we need to test for local host
            // TODO: if (Configuration.Configuration.FileNotFoundHandlerMode == FileNotFoundMode.RemoteOnly)
            if (false)
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
            // TODO:	bool isLoop = IsInfiniteLoop(ctx);
            bool isLoop = false;
            if (isLoop)
                return;

            string newUrl;
            if (Custom404Handler.HandleRequest(GetReferer(ctx.Request.UrlReferrer), notFoundUri, out newUrl))
            {
                ctx.Response.RedirectPermanent(newUrl);
            }
            return;

            // TODO: Verify that we do not need the code below, but instead use customErrors in web.config.

            // Check type of exceptions we can handle
            //   Handles aspx files
            //   Handles EPiServer.PageNotFoundException
            //   Handles Resource Not Found ASP.NET exceptions
            // The outermost exception should be HttpUnhandledException, the inner exception
            // is the one we're interested in
            if (exception != null)
            {
                Exception innerEx = exception.GetBaseException();
                if (innerEx != null && isAspError)
                {
                    ctx.Response.StatusCode = 404;
                    ctx.Response.Status = "404 File not found";
                    if (innerEx is PageNotFoundException)
                    {
                        // Should be a normal 404 handler
                        if (_log.IsInfoEnabled)
                            _log.InfoFormat("404 PageNotFoundException - Url: {0}", notFoundUri.ToString());
                        if (_log.IsDebugEnabled)
                            _log.DebugFormat("404 PageNotFoundException - Exception: {0}", innerEx.ToString());

                        // Redirect to page, handling this as a normal 404 error
                        PerformRedirect(ctx);
                    }

                    // IO File not Found exceptions means the .aspx file cannot
                    // be found. We'll handle this as a standard 404 error
                    if (innerEx is FileNotFoundException)
                    {
                        if (_log.IsInfoEnabled)
                            _log.InfoFormat("404 FileNotFoundException - Url: {0}", notFoundUri.ToString());
                        if (_log.IsDebugEnabled)
                            _log.DebugFormat("404 FileNotFoundException - Exception: {0}", innerEx.ToString());
                        // Redirect to page, handling this as a normal 404 error
                        PerformRedirect(ctx);
                    }

                    // Not all exceptions we need to handle are specific exception types.
                    // We need to handle file not founds, for .aspx pages in directories
                    // that does not exists. However, an 404 error will be returned by the
                    // HttpException class.
                    if (innerEx is HttpException)
                    {
                        HttpException httpEx = innerEx as HttpException;
                        if (httpEx.GetHttpCode() == 404)
                        {
                            if (_log.IsInfoEnabled)
                                _log.InfoFormat("404 HttpException - Url: {0}", notFoundUri.ToString());
                            if (_log.IsDebugEnabled)
                                _log.DebugFormat("404 HttpException - Exception: {0}", httpEx.ToString());

                            PerformRedirect(ctx);
                        }
                    }
                }
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
            string fnfPageUrl = "";//Get404Url();
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
                    string hostUrl = EPiServer.Configuration.Settings.Instance.SiteUrl.ToString();
                    if (refererUrl.StartsWith(hostUrl))
                        refererUrl = refererUrl.Remove(0, hostUrl.Length);
                }
            }
            return refererUrl;
        }

        private static void PerformRedirect(HttpContext ctx)
        {
            // Indicate to IIS 7 that this is a special case
            ctx.ClearError();
            ctx.Response.Clear();

            string url = Get404Url();

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Performing 404 Redirect to: '{0}'", url);

            // if this is a friendly url address, we're unable to use server.transfer
            if (Configuration.Configuration.FileNotFoundHandlerPage.EndsWith(".aspx"))
            {
                ctx.Response.Redirect(url);
            }
            ctx.Server.Transfer(url);
            ctx.Response.End();
            HttpContext.Current = null;
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
            return String.Format("{0}?{1}={2}", baseUrl, ASPX_ERROR_PARAM, HttpContext.Current.Server.UrlEncode(currentUrl));
        }
    }
}