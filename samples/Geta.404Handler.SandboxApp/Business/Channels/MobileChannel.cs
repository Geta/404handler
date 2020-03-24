using System.Web;
using System.Web.WebPages;
using EPiServer.Web;

namespace Geta._404Handler.SandboxApp.Business.Channels
{
    //<summary>
    //Defines the 'Mobile' content channel
    //</summary>
    public class MobileChannel : DisplayChannel
    {
        public const string Name = "mobile";

        public override string ChannelName
        {
            get
            {
                return Name;
            }
        }

        public override string ResolutionId
        {
            get
            {
                return typeof(IphoneVerticalResolution).FullName;
            }
        }

        public override bool IsActive(HttpContextBase context)
        {
            return context.GetOverriddenBrowser().IsMobileDevice;
        }
    }
}
