using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web;
using EPiServer.Logging;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;
using EPiServer.Core;
using EPiServer.Web;
using IPAddress = System.Net.IPAddress;

namespace BVNetwork.NotFound.Core
{
    public class Custom404Handler
    {

        private static readonly List<string> _ignoredResourceExtensions = Configuration.Configuration.IgnoredResourceExtensions;

        private static readonly ILogger Logger = LogManager.GetLogger();

        public static bool HandleRequest(string referer, Uri urlNotFound, out CustomRedirect foundRedirect)
        {
            // Try to match the requested url my matching it
            // to the static list of custom redirects
            CustomRedirectHandler fnfHandler = CustomRedirectHandler.Current;
            CustomRedirect redirect = fnfHandler.CustomRedirects.Find(urlNotFound);
            string pathAndQuery = urlNotFound.PathAndQuery;
            foundRedirect = null;
            if (redirect == null)
            {
                redirect = fnfHandler.CustomRedirects.FindInProviders(urlNotFound.AbsoluteUri);
            }

            if (redirect != null)
            {
                // Url has been deleted from this site
                if (redirect.State.Equals((int)DataStoreHandler.State.Deleted))
                {
                    foundRedirect = redirect;
                    return true;
                }

                if (redirect.State.Equals((int)DataStoreHandler.State.Saved))
                {
                    // Found it, however, we need to make sure we're not running in an
                    // infinite loop. The new url must not be the referrer to this page
                    if (string.Compare(redirect.NewUrl, pathAndQuery, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {

                        foundRedirect = redirect;
                        return true;
                    }
                }
            }
            else
            {
                // log request to database - if logging is turned on.
                if (Configuration.Configuration.Logging == LoggerMode.On)
                {
                    // Safe logging
                    RequestLogger.Instance.LogRequest(pathAndQuery, referer);
                }
            }
            return false;
        }

        public static void FileNotFoundExceptionHandler(object sender, EventArgs e)
        {
            HttpContext context = GetContext();
            if (context == null)
                return;
            if (CheckForException(context, context.Request.Url))
            {
                context.Response.Clear();
                context.Response.TrySkipIisCustomErrors = true;
                context.Server.ClearError();
                context.Response.StatusCode = 404;
            }
        }

        private static HttpContext GetContext()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                Logger.Debug("No HTTPContext, returning");
            }
            return context;
        }

        public static void FileNotFoundHandler(object sender, EventArgs evt)
        {
            // Check if this should be enabled
            if (Configuration.Configuration.FileNotFoundHandlerMode == FileNotFoundMode.Off)
                return;

            HttpContext context = GetContext();
            if (context == null)
                return;

            if (context.Response.StatusCode != 404)
                return;

            // If we're only doing this for remote users, we need to test for local host
            if (Configuration.Configuration.FileNotFoundHandlerMode == FileNotFoundMode.RemoteOnly)
            {
                // Determine if we're on localhost
                bool localHost = IsLocalhost();
                if (localHost)
                {
                    Logger.Debug("Determined to be localhost, returning");
                    return;
                }
                Logger.Debug("Not localhost, handling error");
            }

            Logger.Debug("FileNotFoundHandler called");

            Uri notFoundUri = context.Request.Url;

            // Skip resource files
            if (IsResourceFile(notFoundUri))
                return;

            string query = context.Request.ServerVariables["QUERY_STRING"];

            // avoid duplicate log entries
            if ((query != null) && query.StartsWith("404;"))
            {
                return;
            }

            CustomRedirect newUrl;
            var canHandleRedirect = HandleRequest(GetReferer(context.Request.UrlReferrer), notFoundUri, out newUrl);
            if (canHandleRedirect && newUrl.State == (int)DataStoreHandler.State.Saved)
            {
                context.Response.Clear();
                context.Response.TrySkipIisCustomErrors = true;
                context.Server.ClearError();
                context.Response.RedirectPermanent(newUrl.NewUrl);
                context.Response.End();
            }
            else if (canHandleRedirect && newUrl.State == (int)DataStoreHandler.State.Deleted)
            {
                SetStatusCodeAndShow404(context, 410);
            }
            else
            {
                SetStatusCodeAndShow404(context, 404);
            }
        }

        protected static void SetStatusCodeAndShow404(HttpContext context, int statusCode = 404)
        {

            context.Response.TrySkipIisCustomErrors = true;
            context.Server.ClearError();
            context.Response.StatusCode = statusCode;
            context.Response.End();
        }

        private static bool CheckForException(HttpContext context, Uri notFoundUri)
        {
            try
            {
                var exception = context.Server.GetLastError();
                if (exception != null)
                {
                    Exception innerEx = exception.GetBaseException();
                    if (innerEx is PageNotFoundException)
                    {
                        // Should be a normal 404 handler
                        Logger.Information("404 PageNotFoundException - Url: {0}", notFoundUri.ToString());
                        Logger.Debug("404 PageNotFoundException - Exception: {0}", innerEx.ToString());
                        return true;
                    }

                    // IO File not Found exceptions means the .aspx file cannot
                    // be found. We'll handle this as a standard 404 error
                    if (innerEx is FileNotFoundException)
                    {
                        Logger.Information("404 FileNotFoundException - Url: {0}", notFoundUri.ToString());
                        Logger.Debug("404 FileNotFoundException - Exception: {0}", innerEx.ToString());
                        return true;
                    }

                    // Not all exceptions we need to handle are specific exception types.
                    // We need to handle file not founds, for .aspx pages in directories
                    // that does not exists. However, an 404 error will be returned by the
                    // HttpException class.
                    HttpException httpEx = innerEx as HttpException;
                    if (httpEx != null)
                    {
                        if (httpEx.GetHttpCode() == 404)
                        {
                            Logger.Information("404 HttpException - Url: {0}", notFoundUri.ToString());
                            Logger.Debug("404 HttpException - Exception: {0}", httpEx.ToString());
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Unable to fetch 404 exception.", ex);
            }
            return false;
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
                    Logger.Debug("Ignoring rewrite of '{0}'. '{1}' is a known resource extension", notFoundUri.ToString(), extension);
                    return true;
                }
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
    }
}