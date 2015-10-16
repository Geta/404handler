using System;
using EPiServer.Web;

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    public class SimplePageNotFoundBase : EPiServer.SimplePage
    {
        private Uri _urlNotFound = null;
        private string _referer = null;
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
                       _urlNotFound = new Uri(SiteDefinition.Current.SiteUrl + NotFoundPageUtil.GetUrlNotFound(this.Page)); ;
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
                    _referer = NotFoundPageUtil.GetReferer(this.Page);
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
