using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer;

namespace Geta._404Handler.SandboxApp.Models.Blocks
{
    /// <summary>
    /// Used to insert a link which is styled as a button
    /// </summary>
    [SiteContentType(GUID = "426CF12F-1F01-4EA0-922F-0778314DDAF0")]
    [SiteImageUrl]
    public class ButtonBlock : SiteBlockData
    {
        [Display(Order = 1, GroupName = SystemTabNames.Content)]
        [Required]
        public virtual string ButtonText { get; set; }

        [Display(Order = 2, GroupName = SystemTabNames.Content)]
        [Required]
        public virtual Url ButtonLink { get; set; }
    }
}
