using System.Reflection;
using System.Web;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Upgrade;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Initialization
{
    /// <summary>
    /// Global File Not Found Handler, for handling Asp.net exceptions
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class Custom404HandlerInitialization : IInitializableHttpModule
    {
        private static readonly ILogger _log = LogManager.GetLogger(typeof(Custom404HandlerInitialization));

        public void Initialize(InitializationEngine context)
        {
            
            _log.Debug("Initializing 404 handler version check");
            DataAccessBaseEx dba = DataAccessBaseEx.GetWorker();
            int version = dba.Check404Version();
            if (version != Configuration.Configuration.CURRENT_VERSION)
            {
                _log.Debug("Older version found. Version nr. :" + version);
                Upgrader.Start(version);
            }
            else
                Upgrader.Valid = true;

            // Load all custom redirects into memory
            CustomRedirectHandler handler = CustomRedirectHandler.Current;
        }

        public void Uninitialize(InitializationEngine context)
        {

        }

        public void Preload(string[] parameters)
        {
        }

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.EndRequest += Custom404Handler.FileNotFoundHandler;
        }
    }
}