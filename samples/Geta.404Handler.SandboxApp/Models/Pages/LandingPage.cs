using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace Geta._404Handler.SandboxApp.Models.Pages
{
    /// <summary>
    /// Used for campaign or landing pages, commonly used for pages linked in online advertising such as AdWords
    /// </summary>
    [SiteContentType(
        GUID = "DBED4258-8213-48DB-A11F-99C034172A54",
        GroupName = Global.GroupNames.Specialized)]
    [SiteImageUrl]
    public class LandingPage : SitePageData
    {
        [Display(
            GroupName = SystemTabNames.Content,
            Order=310)]
        [CultureSpecific]
        public virtual ContentArea MainContentArea { get; set; }

        public override void SetDefaultValues(ContentType contentType)
        {
            base.SetDefaultValues(contentType);

            HideSiteFooter = true;
            HideSiteHeader = true;
        }
    }
}
