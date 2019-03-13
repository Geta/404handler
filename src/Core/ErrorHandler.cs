using System;
using System.IO;
using System.Web;
using BVNetwork.NotFound.Core.Web;
using EPiServer.Core;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core
{
    public class ErrorHandler
    {
        private readonly RequestHandler _requestHandler;
        private static readonly ILogger Logger = LogManager.GetLogger();

        public ErrorHandler(RequestHandler requestHandler)
        {
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
        }

        public virtual void Handle(HttpContextBase context)
        {
            if (context == null) return;

            if (IsNotFoundException(context.Server.GetLastError(), context.Request.Url))
            {
                context
                    .ClearServerError()
                    .SetStatusCode(404);
                _requestHandler.Handle(context);
            }
        }

        public virtual bool IsNotFoundException(Exception exception, Uri notFoundUri)
        {
            if (exception == null) return false;
            if (notFoundUri == null) return false;

            try
            {
                var innerEx = exception.GetBaseException();
                switch (innerEx)
                {
                    case ContentNotFoundException _:
                        // Should be a normal 404 handler
                        Logger.Information("404 ContentNotFoundException - Url: {0}", notFoundUri.ToString());
                        Logger.Debug("404 ContentNotFoundException - Exception: {0}", innerEx.ToString());
                        return true;
                    case FileNotFoundException _:
                        Logger.Information("404 FileNotFoundException - Url: {0}", notFoundUri.ToString());
                        Logger.Debug("404 FileNotFoundException - Exception: {0}", innerEx.ToString());
                        return true;
                    case HttpException httpEx:
                        if (httpEx.GetHttpCode() == 404)
                        {
                            Logger.Information("404 HttpException - Url: {0}", notFoundUri.ToString());
                            Logger.Debug("404 HttpException - Exception: {0}", httpEx.ToString());
                            return true;
                        }
                        break;

                        // IO File not Found exceptions means the .aspx file cannot
                        // be found. We'll handle this as a standard 404 error

                        // Not all exceptions we need to handle are specific exception types.
                        // We need to handle file not founds, for .aspx pages in directories
                        // that does not exists. However, an 404 error will be returned by the
                        // HttpException class.
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Unable to fetch 404 exception.", ex);
            }
            return false;
        }
    }
}