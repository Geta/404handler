// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Web;

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    public class SimplePageNotFoundBase : EPiServer.SimplePage
    {
        private Uri _urlNotFound;
        private string _referer;
        private PageContent _content;

        /// <summary>
        /// Content for the page
        /// </summary>
        /// <value></value>
        public PageContent Content => _content ?? (_content = NotFoundPageUtil.Get404PageLanguageResourceContent());

        /// <summary>
        /// Holds the url - if any - the user tried to find
        /// </summary>
        public Uri UrlNotFound => _urlNotFound
                                    ?? (_urlNotFound =
                                        new Uri(NotFoundPageUtil.GetUrlNotFound(new HttpRequestWrapper(Page.Request))));

        /// <summary>
        /// The refering url
        /// </summary>
        public string Referer => _referer ?? (_referer = NotFoundPageUtil.GetReferer(new HttpRequestWrapper(Page.Request)));

        /// <inheritdoc />
        /// <summary>
        /// Load event for the page
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            NotFoundPageUtil.HandleOnLoad(Page, UrlNotFound, Referer);
        }
    }
}
