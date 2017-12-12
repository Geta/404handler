using System.Web;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Upgrade;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;

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
        private static readonly ILogger Log = LogManager.GetLogger(typeof(Custom404HandlerInitialization));

        public void Initialize(InitializationEngine context)
        {

            Log.Debug("Initializing 404 handler version check");
            var dba = DataAccessBaseEx.GetWorker();
            var version = dba.Check404Version();
            if (version != Configuration.Configuration.CurrentVersion)
            {
                Log.Debug("Older version found. Version nr. :" + version);
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
            application.Error += Custom404Handler.FileNotFoundExceptionHandler;
            application.EndRequest += Custom404Handler.FileNotFoundHandler;
        }
    }
}