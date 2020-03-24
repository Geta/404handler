using System.Web;
using System.Web.Mvc;
using EPiServer.Core;
using Geta._404Handler.SandboxApp.Helpers;
using Geta._404Handler.SandboxApp.Models.Blocks;
using Geta._404Handler.SandboxApp.Models.Pages;
using Geta._404Handler.SandboxApp.Models.ViewModels;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using EPiServer;

namespace Geta._404Handler.SandboxApp.Controllers
{
    public class ContactBlockController : BlockController<ContactBlock>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPermanentLinkMapper _permanentLinkMapper;

        public ContactBlockController(IContentLoader contentLoader, IPermanentLinkMapper permanentLinkMapper)
        {
            _contentLoader = contentLoader;
            _permanentLinkMapper = permanentLinkMapper;
        }

        public override ActionResult Index(ContactBlock currentBlock)
        {
            ContactPage contactPage = null;
            if(!ContentReference.IsNullOrEmpty(currentBlock.ContactPageLink))
            {
                contactPage = _contentLoader.Get<ContactPage>(currentBlock.ContactPageLink);
            }

            var linkUrl = GetLinkUrl(currentBlock);

            var model = new ContactBlockModel
                {
                    Heading = currentBlock.Heading,
                    Image = currentBlock.Image,
                    ContactPage = contactPage,
                    LinkUrl = GetLinkUrl(currentBlock),
                    LinkText = currentBlock.LinkText,
                    ShowLink = linkUrl != null
                };

            //As we're using a separate view model with different property names than the content object
            //we connect the view models properties with the content objects so that they can be edited.
            ViewData.GetEditHints<ContactBlockModel, ContactBlock>()
                .AddConnection(x => x.Heading, x => x.Heading)
                .AddConnection(x => x.Image, x => x.Image)
                .AddConnection(x => (object) x.ContactPage, x => (object) x.ContactPageLink)
                .AddConnection(x => x.LinkText, x => x.LinkText);

            return PartialView(model);
        }

        private IHtmlString GetLinkUrl(ContactBlock contactBlock)
        {
            if (contactBlock.LinkUrl != null && !contactBlock.LinkUrl.IsEmpty())
            {
                var linkUrl = contactBlock.LinkUrl.ToString();

                //If the url maps to a page on the site we convert it from the internal (permanent, GUID-like) format
                //to the human readable and pretty public format
                var linkMap = _permanentLinkMapper.Find(new UrlBuilder(linkUrl));
                if (linkMap != null && !ContentReference.IsNullOrEmpty(linkMap.ContentReference))
                {
                    return Url.PageLinkUrl(linkMap.ContentReference);
                }

                return new MvcHtmlString(contactBlock.LinkUrl.ToString());
            }

            return null;
        }

    }
}
