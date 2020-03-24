using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Geta._404Handler.SandboxApp.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DisplayRegistryInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            if (context.HostType == HostType.WebApplication)
            {
                // Register Display Options
                var options = ServiceLocator.Current.GetInstance<DisplayOptions>();
                options
                    .Add("full", "/displayoptions/full", Global.ContentAreaTags.FullWidth, "", "epi-icon__layout--full")
                    .Add("wide", "/displayoptions/wide", Global.ContentAreaTags.TwoThirdsWidth, "", "epi-icon__layout--two-thirds")
                    .Add("narrow", "/displayoptions/narrow", Global.ContentAreaTags.OneThirdWidth, "", "epi-icon__layout--one-third");

                AreaRegistration.RegisterAllAreas();

            }
        }

        public void Preload(string[] parameters){}

        public void Uninitialize(InitializationEngine context){}
    }
}
