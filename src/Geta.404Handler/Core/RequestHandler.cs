// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Diagnostics;
using System.Net;
using System.Web;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;
using BVNetwork.NotFound.Core.Web;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core
{
    public class RequestHandler
    {
        private readonly IRedirectHandler _redirectHandler;
        private readonly IRequestLogger _requestLogger;
        private readonly IConfiguration _configuration;
        private const string HandledRequestItemKey = "404handler:handled";

        private static readonly ILogger Logger = LogManager.GetLogger();

        public RequestHandler(
            IRedirectHandler redirectHandler,
            IRequestLogger requestLogger,
            IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _requestLogger = requestLogger ?? throw new ArgumentNullException(nameof(requestLogger));
            _redirectHandler = redirectHandler ?? throw new ArgumentNullException(nameof(redirectHandler));
        }

        public virtual void Handle(HttpContextBase context)
        {
            if (context == null) return;

            if (IsHandled(context))
            {
                LogDebug("Already handled.", context);
                return;
            }

            if (context.Response.StatusCode != 404)
            {
                LogDebug("Not a 404 response.", context);
                return;
            }

            if (_configuration.FileNotFoundHandlerMode == FileNotFoundMode.Off)
            {
                LogDebug("Not handled, custom redirect manager is set to off.", context);
                return;
            }
            // If we're only doing this for remote users, we need to test for local host
            if (_configuration.FileNotFoundHandlerMode == FileNotFoundMode.RemoteOnly)
            {
                // Determine if we're on localhost
                var localHost = IsLocalhost(context);
                if (localHost)
                {
                    LogDebug("Determined to be localhost, returning.", context);
                    return;
                }
                LogDebug("Not a localhost, handling error.", context);
            }

            LogDebug("Handling 404 request.", context);

            var notFoundUri = context.Request.Url;

            if (IsResourceFile(notFoundUri))
            {
                LogDebug("Skipping resource file.", context);
                return;
            }

            var query = context.Request.ServerVariables["QUERY_STRING"];

            // avoid duplicate log entries
            if (query != null && query.StartsWith("404;"))
            {
                LogDebug("Skipping request with 404; in the query string.", context);
                return;
            }

            var canHandleRedirect = HandleRequest(context.Request.UrlReferrer, notFoundUri, out var newUrl);
            if (canHandleRedirect && newUrl.State == (int)RedirectState.Saved)
            {
                LogDebug("Handled saved URL", context);

                context.ClearServerError()
                        .Redirect(newUrl.NewUrl, newUrl.RedirectType);
            }
            else if (canHandleRedirect && newUrl.State == (int)RedirectState.Deleted)
            {
                LogDebug("Handled deleted URL", context);

                SetStatusCodeAndShow404(context, 410);
            }
            else
            {
                LogDebug("Not handled. Current URL is ignored or no redirect found.", context);

                SetStatusCodeAndShow404(context);
            }

            MarkHandled(context);

        }

        private bool IsHandled(HttpContextBase context)
        {
            return context.Items.Contains(HandledRequestItemKey)
                && (bool)context.Items[HandledRequestItemKey];
        }

        private void MarkHandled(HttpContextBase context)
        {
            context.Items[HandledRequestItemKey] = true;
        }

        public virtual bool HandleRequest(Uri referrer, Uri urlNotFound, out CustomRedirect foundRedirect)
        {
            var redirect = _redirectHandler.Find(urlNotFound);

            foundRedirect = null;

            if (redirect != null)
            {
                // Url has been deleted from this site
                if (redirect.State.Equals((int)RedirectState.Deleted))
                {
                    foundRedirect = redirect;
                    return true;
                }

                if (redirect.State.Equals((int)RedirectState.Saved))
                {
                    // Found it, however, we need to make sure we're not running in an
                    // infinite loop. The new url must not be the referrer to this page
                    if (string.Compare(redirect.NewUrl, urlNotFound.PathAndQuery, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {

                        foundRedirect = redirect;
                        return true;
                    }
                }
            }
            else
            {
                // log request to database - if logging is turned on.
                if (_configuration.Logging == LoggerMode.On)
                {
                    // Safe logging
                    var logUrl = _configuration.LogWithHostname ? urlNotFound.ToString() : urlNotFound.PathAndQuery;
                    _requestLogger.LogRequest(logUrl, referrer?.ToString());
                }
            }
            return false;
        }

        public virtual void SetStatusCodeAndShow404(HttpContextBase context, int statusCode = 404)
        {
            context
                .ClearServerError()
                .SetStatusCode(statusCode);
        }

        /// <summary>
        /// Determines whether the specified not found URI is a resource file
        /// </summary>
        /// <param name="notFoundUri">The not found URI.</param>
        /// <returns>
        /// <c>true</c> if it is a resource file; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsResourceFile(Uri notFoundUri)
        {
            var extension = notFoundUri.AbsolutePath;
            var extPos = extension.LastIndexOf('.');

            if (extPos <= 0) return false;

            extension = extension.Substring(extPos + 1);
            if (_configuration.IgnoredResourceExtensions.Contains(extension))
            {
                // Ignoring 404 rewrite of known resource extension
                Logger.Debug("Ignoring rewrite of '{0}'. '{1}' is a known resource extension", notFoundUri.ToString(), extension);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the current request is on localhost.
        /// </summary>
        /// <returns>
        /// <c>true</c> if current request is localhost; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsLocalhost(HttpContextBase context)
        {
            try
            {
                var hostAddress = context.Request.UserHostAddress ?? string.Empty;
                var address = IPAddress.Parse(hostAddress);
                Debug.WriteLine("IP Address of user: " + address, "404Handler");

                var host = Dns.GetHostEntry(Dns.GetHostName());
                Debug.WriteLine("Host Entry of local computer: " + host.HostName, "404Handler");
                return address.Equals(IPAddress.Loopback) || Array.IndexOf(host.AddressList, address) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private void LogDebug(string message, HttpContextBase context)
        {
            Logger.Debug(
                $"{{0}}{Environment.NewLine}Request URL: {{1}}{Environment.NewLine}Response status code: {{2}}",
                message, context?.Request.Url, context?.Response.StatusCode);
        }
    }
}
