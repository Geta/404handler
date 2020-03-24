using System.Linq;
using System.Web;
using System.Web.WebPages;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Geta._404Handler.SandboxApp.Business.Channels;
using EPiServer.Web;

namespace Geta._404Handler.SandboxApp.Business.Initialization
{
    /// <summary>
    /// Adds a new display mode for mobile which is active if the mobile channel is active in addition to if the request is from a mobile device (like the default one)
    /// </summary>
    /// <remarks>
    /// It's also possible to map a display mode as a channel through the DisplayChannelService.RegisterDisplayMode() method.
    /// Adding channels that way does not however enable specifying ResolutionId which we want to do for the mobile channel.
    /// </remarks>
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DisplayModesInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var mobileChannelDisplayMode = new DefaultDisplayMode("mobile")
            {
                ContextCondition = IsMobileDisplayModeActive
            };
            DisplayModeProvider.Instance.Modes.Insert(0, mobileChannelDisplayMode);
        }

        private static bool IsMobileDisplayModeActive(HttpContextBase httpContext)
        {
            if (httpContext.GetOverriddenBrowser().IsMobileDevice)
            {
                return true;
            }
            var displayChannelService = ServiceLocator.Current.GetInstance<IDisplayChannelService>();
            return displayChannelService.GetActiveChannels(httpContext).Any(x => x.ChannelName == MobileChannel.Name);
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
