// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using BVNetwork.NotFound.Core.Data;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    [EPiServerDataStore(AutomaticallyRemapStore = true, StoreName = "BVNetwork.FileNotFound.Redirects.CustomRedirect")]
    public class CustomRedirect : IDynamicData
    {
        private string _oldUrl;
        public int NotfoundErrorCount;

        /// <summary>
        /// Gets or sets a value indicating whether to skip appending the
        /// old url fragment to the new one. Default value is false.
        /// </summary>
        /// <remarks>
        /// If you want to redirect many addresses below a specific one to
        /// one new url, set this to true. If we get a wild card match on
        /// this url, the new url will be used in its raw format, and the
        /// old url will not be appended to the new one.
        /// </remarks>
        /// <value><c>true</c> to skip appending old url if wild card match; otherwise, <c>false</c>.</value>
        public bool WildCardSkipAppend { get; set; }

        public string OldUrl
        {
            get => _oldUrl;
            set => _oldUrl = value?.ToLower();
        }

        public int OldUrlElementLength
        {
            get
            {
                return _oldUrl.Trim('/').Split('/').Length;
            }
        }

        public string NewUrl { get; set; }

        public int  State { get; set; }

        // 301 (permanent) or 302 (temporary)
        public RedirectType RedirectType { get; set; }

        /// <summary>
        /// Tells if the new url is a virtual url, not containing
        /// the base root url to redirect to. All urls starting with
        /// "/" is determined to be virtuals.
        /// </summary>
        public bool IsVirtual => NewUrl.StartsWith("/");

        /// <summary>
        /// The hash code for the CustomRedirect class is the
        /// old url string, which is the one we'll be doing lookups
        /// based on.
        /// </summary>
        /// <returns>The Hash code of the old Url</returns>
        public override int GetHashCode()
        {
            return OldUrl != null ? OldUrl.GetHashCode() : 0;
        }

        public Identity Id { get; set; }

        public CustomRedirect()
        {
        }

        public CustomRedirect(string oldUrl, string newUrl, bool skipWildCardAppend, RedirectType redirectType)
            : this(oldUrl, newUrl)
        {
            WildCardSkipAppend = skipWildCardAppend;
            RedirectType = redirectType;
        }

        public CustomRedirect(string oldUrl, string newUrl)
        {
            OldUrl = oldUrl;
            NewUrl = newUrl;
        }

        public CustomRedirect(string oldUrl, int state, int count)
        {
            OldUrl = oldUrl;
            State = state;
            NotfoundErrorCount = count;
        }

        public CustomRedirect(CustomRedirect redirect)
        {
            OldUrl = redirect._oldUrl;
            NewUrl = redirect.NewUrl;
            WildCardSkipAppend = redirect.WildCardSkipAppend;
            RedirectType = redirect.RedirectType;
        }
    }
}
