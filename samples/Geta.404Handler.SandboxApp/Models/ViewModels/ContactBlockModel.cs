using System.Web;
using System.ComponentModel.DataAnnotations;
using Geta._404Handler.SandboxApp.Models.Pages;
using EPiServer.Web;
using EPiServer.Core;

namespace Geta._404Handler.SandboxApp.Models.ViewModels
{
    public class ContactBlockModel
    {
        [UIHint(UIHint.Image)]
        public ContentReference Image { get; set; }
        public string Heading { get; set; }
        public string LinkText { get; set; }
        public IHtmlString LinkUrl { get; set; }
        public bool ShowLink { get; set; }
        public ContactPage ContactPage { get; set; }
    }
}
