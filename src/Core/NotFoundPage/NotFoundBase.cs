using System;
using System.Web;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    public class NotFoundBase : System.Web.UI.Page
    {
        private Uri _urlNotFound;
        private string _referer;
        PageContent _content;

        /// <summary>
        /// Content for the page
        /// </summary>
        /// <value></value>
        public PageContent Content
        {
            get
            {
                if (_content == null)
                    _content = NotFoundPageUtil.Get404PageLanguageResourceContent();
                return _content;
            }
        }

        /// <summary>
        /// Holds the url - if any - the user tried to find
        /// </summary>
        public Uri UrlNotFound
        {
            get
            {
                if (_urlNotFound == null)
                {
                    _urlNotFound = new Uri(NotFoundPageUtil.GetUrlNotFound(new HttpRequestWrapper(Page.Request)));
                }
                return _urlNotFound;
            }
        }

        /// <summary>
        /// The refering url
        /// </summary>
        public string Referer
        {
            get
            {
                if (_referer == null)
                {
                    _referer = HttpUtility.HtmlEncode(NotFoundPageUtil.GetReferer(new HttpRequestWrapper(Page.Request)));
                }
                return _referer;
            }
        }

        /// <summary>
        /// Load event for the page
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            NotFoundPageUtil.HandleOnLoad(this.Page, UrlNotFound, this.Referer);
        }
    }
}
