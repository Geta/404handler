using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Framework;
using BVNetwork.FileNotFound.Redirects;
using EPiServer.Framework.Initialization;
using BVNetwork.Bvn.FileNotFound.Data;
using BVNetwork.Bvn.FileNotFound.Upgrade;
using log4net;

namespace BVNetwork.FileNotFound.Redirects
{
    
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class CustomRedirectInitialization : IInitializableModule
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void Initialize(InitializationEngine context)
        {
            EPiServer.Web.InitializationModule.FirstBeginRequest += new EventHandler(ApplicationFirstBeginRequest);

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
            
        } 

        // When you receive the FirstBeginRequest event it is safe to access the EPi Internal objects.
        private void ApplicationFirstBeginRequest(object sender, EventArgs e)
        {
            // Load all custom redirects into memory
            CustomRedirectHandler handler = CustomRedirectHandler.Current;
        }

        public void Uninitialize(InitializationEngine context)
        {
            throw new NotImplementedException();
        }

        public void Preload(string[] parameters)
        {
            throw new NotImplementedException();
        }

    }
}
