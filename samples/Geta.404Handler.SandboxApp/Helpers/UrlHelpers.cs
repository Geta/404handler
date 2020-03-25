using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Core;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer;

namespace Geta._404Handler.SandboxApp.Helpers
{
    public static class UrlHelpers
    {
        /// <summary>
        /// Returns the target URL for a ContentReference. Respects the page's shortcut setting
        /// so if the page is set as a shortcut to another page or an external URL that URL
        /// will be returned.
        /// </summary>
        public static IHtmlString PageLinkUrl(this UrlHelper urlHelper, ContentReference contentLink)
        {
            if(ContentReference.IsNullOrEmpty(contentLink))
            {
                return MvcHtmlString.Empty;
            }

            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var page = contentLoader.Get<PageData>(contentLink);

            return PageLinkUrl(urlHelper, page);
        }

        /// <summary>
        /// Returns the target URL for a page. Respects the page's shortcut setting
        /// so if the page is set as a shortcut to another page or an external URL that URL
        /// will be returned.
        /// </summary>
        public static IHtmlString PageLinkUrl(this UrlHelper urlHelper, PageData page)
        {
            var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
            switch (page.LinkType)
            {
                case PageShortcutType.Normal:
                case PageShortcutType.FetchData:
                    return new MvcHtmlString(urlResolver.GetUrl(page.ContentLink));

                case PageShortcutType.Shortcut:
                    var shortcutProperty = page.Property["PageShortcutLink"] as PropertyPageReference;
                    if (shortcutProperty != null && !ContentReference.IsNullOrEmpty(shortcutProperty.ContentLink))
                    {
                        return urlHelper.PageLinkUrl(shortcutProperty.ContentLink);
                    }
                    break;

                case PageShortcutType.External:
                    return new MvcHtmlString(page.LinkURL);
            }
            return MvcHtmlString.Empty;
        }

        public static RouteValueDictionary GetPageRoute(this RequestContext requestContext, ContentReference contentLink)
        {
            var values = new RouteValueDictionary();
            values[RoutingConstants.NodeKey] = contentLink;
            values[RoutingConstants.LanguageKey] = ContentLanguage.PreferredCulture.Name;
            return values;
        }
    }
}
