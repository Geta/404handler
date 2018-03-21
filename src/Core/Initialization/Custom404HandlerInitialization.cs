using System;
using System.Web;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Upgrade;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace BVNetwork.NotFound.Core.Initialization
{
    /// <inheritdoc />
    /// <summary>
    /// Global File Not Found Handler, for handling Asp.net exceptions
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class Custom404HandlerInitialization : IInitializableHttpModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private static Injected<RequestHandler> RequestHandler { get; set; }
        private static Injected<ErrorHandler> ErrorHandler { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        public void Initialize(InitializationEngine context)
        {

            Logger.Debug("Initializing 404 handler version check");
            var dba = DataAccessBaseEx.GetWorker();
            var version = dba.Check404Version();
            if (version != Configuration.Configuration.CurrentVersion)
            {
                Logger.Debug("Older version found. Version nr. :" + version);
                Upgrader.Start(version);
            }
            else
            {
                Upgrader.Valid = true;
            }

            // Load all custom redirects into memory
            // TODO: create better load of the cache
            var handler = CustomRedirectHandler.Current;
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.Error += OnError;
            application.EndRequest += OnEndRequest;
        }

        private void OnEndRequest(object sender, EventArgs eventArgs)
        {
            try
            {
                RequestHandler.Service.Handle(GetContext());
            }
            catch (Exception e)
            {
                Logger.Error("Error on 404 handling.", e);
                throw;
            }
        }

        private void OnError(object sender, EventArgs eventArgs)
        {
            try
            {
                ErrorHandler.Service.Handle(GetContext());
            }
            catch (Exception e)
            {
                Logger.Error("Error on 404 handling.", e);
                throw;
            }
        }

        private static HttpContextBase GetContext()
        {
            var context = HttpContext.Current;
            if (context != null) return new HttpContextWrapper(context);

            Logger.Debug("No HTTPContext, returning");
            return null;
        }
    }
}