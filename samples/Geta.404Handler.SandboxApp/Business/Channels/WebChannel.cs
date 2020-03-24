using System.Web;
using EPiServer.Web;

namespace Geta._404Handler.SandboxApp.Business.Channels
{
    /// <summary>
    /// Defines the 'Web' content channel
    /// </summary>
    public class WebChannel : DisplayChannel
    {
        public override string ChannelName
        {
            get
            {
                return "web";
            }
        }

        public override bool IsActive(HttpContextBase context)
        {
            return !context.Request.Browser.IsMobileDevice;
        }
    }
}
