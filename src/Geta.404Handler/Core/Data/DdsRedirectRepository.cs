// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using BVNetwork.NotFound.Core.CustomRedirects;
using EPiServer.Data.Dynamic;

namespace BVNetwork.NotFound.Core.Data
{
    public class DdsRedirectRepository : IRepository<CustomRedirect>, IRedirectLoader
    {
        private DynamicDataStore Store => DataStoreFactory.GetStore(typeof(CustomRedirect));
        private const string OldUrlPropertyName = "OldUrl";

        public void Save(CustomRedirect customRedirect)
        {
            Store.Save(customRedirect);
        }

        public void Delete(CustomRedirect customRedirect)
        {
            Store.Delete(customRedirect);
        }

        public CustomRedirect GetByOldUrl(string oldUrl)
        {
            return Store.Find<CustomRedirect>(OldUrlPropertyName, oldUrl.ToLower()).SingleOrDefault();
        }

        public IEnumerable<CustomRedirect> GetAll()
        {
            return Store.Items<CustomRedirect>().OrderBy(x => x.OldUrl);
        }

        public IEnumerable<CustomRedirect> GetByState(RedirectState state)
        {
            return Store.Items<CustomRedirect>()
                .OrderBy(cr => cr.OldUrl)
                .Where(s => s.State.Equals((int) state));
        }

        public IEnumerable<CustomRedirect> Find(string searchText)
        {
            return Store.Items<CustomRedirect>()
                .Where(s => s.NewUrl.Contains(searchText) || s.OldUrl.Contains(searchText));
        }
    }
}