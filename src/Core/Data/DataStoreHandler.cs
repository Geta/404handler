using System;
using System.Collections.Generic;
using System.Linq;
using BVNetwork.NotFound.Core.CustomRedirects;
using EPiServer.Data;

namespace BVNetwork.NotFound.Core.Data
{
    public class DataStoreHandler : IRedirectsService
    {
        private readonly IRepository<CustomRedirect, Identity> _repository;
        private readonly IRedirectLoader _redirectLoader;

        public DataStoreHandler()
        {
            var repository = new DdsRedirectRepository();
            _repository = repository;
            _redirectLoader = repository;
        }

        public IEnumerable<CustomRedirect> GetAll()
        {
            return GetCustomRedirects(false);
        }

        public IEnumerable<CustomRedirect> GetAllExcludingIgnored()
        {
            return GetCustomRedirects(true);
        }

        public IEnumerable<CustomRedirect> GetIgnored()
        {
            return GetIgnoredRedirect();
        }

        public IEnumerable<CustomRedirect> GetDeleted()
        {
            return GetDeletedRedirect();
        }

        public IEnumerable<CustomRedirect> Search(string searchText)
        {
            return SearchCustomRedirects(searchText);
        }

        public void AddOrUpdate(CustomRedirect redirect)
        {
            SaveCustomRedirect(redirect);
        }

        public void DeleteByOldUrl(string oldUrl)
        {
            DeleteCustomRedirect(oldUrl);
        }

        public int DeleteAll()
        {
            return DeleteAllCustomRedirectsInternal();
        }

        public int DeleteAllIgnored()
        {
            return DeleteAllIgnoredRedirects();
        }

        private const string OldUrlPropertyName = "OldUrl";

        [Obsolete]
        public void SaveCustomRedirect(CustomRedirect currentCustomRedirect)
        {
            var match = _redirectLoader.GetByOldUrl(currentCustomRedirect.OldUrl);

            //if there is a match, replace the value.
            if (match != null)
            {
                currentCustomRedirect.Id = match.Id;
            }
            _repository.Save(currentCustomRedirect);
        }

        /// <summary>
        /// Returns a list of all CustomRedirect objects in the Dynamic Data Store.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public List<CustomRedirect> GetCustomRedirects(bool excludeIgnored)
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            IEnumerable<CustomRedirect> customRedirects;
            if (excludeIgnored)
            {
                customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                                  where s.State.Equals((int)RedirectState.Saved)
                                  select s;
            }
            else
            {
                customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                                  select s;
            }
            return customRedirects.ToList();
        }

        [Obsolete]
        public List<CustomRedirect> GetIgnoredRedirect()
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            var customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                              where s.State.Equals(RedirectState.Ignored)
                              select s;
            return customRedirects.ToList();
        }

        [Obsolete]
        public List<CustomRedirect> GetDeletedRedirect()
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            var deletedRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                              where s.State.Equals(RedirectState.Deleted)
                              select s;
            return deletedRedirects.ToList();
        }

        [Obsolete]
        public void UnignoreRedirect()
        {
        }

        /// <summary>
        /// Delete CustomObject object from Data Store that has given "OldUrl" property
        /// </summary>
        [Obsolete]
        public void DeleteCustomRedirect(string oldUrl)
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            var match = store.Find<CustomRedirect>(OldUrlPropertyName, oldUrl.ToLower()).SingleOrDefault();
            if (match != null)
            {
                store.Delete(match);
            }
        }

        /// <summary>
        /// Delete all CustomRedirect objects
        /// </summary>
        [Obsolete]
        public void DeleteAllCustomRedirects()
        {
            DeleteAllCustomRedirectsInternal();
        }

        private int DeleteAllCustomRedirectsInternal()
        {
            // In order to avoid a database timeout, we delete the items one by one.
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            var redirects = GetAll().ToList();
            foreach (var redirect in redirects)
            {
                store.Delete(redirect);
            }
            return redirects.Count;
        }

        [Obsolete]
        public int DeleteAllIgnoredRedirects()
        {
            // In order to avoid a database timeout, we delete the items one by one.
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            var ignoredRedirects = GetIgnored().ToList();
            foreach (var redirect in ignoredRedirects)
            {
                store.Delete(redirect);
            }
            return ignoredRedirects.Count;
        }

        /// <summary>
        /// Find all CustomRedirect objects which has a OldUrl og NewUrl that contains the search word.
        /// </summary>
        /// <param name="searchWord"></param>
        /// <returns></returns>
        [Obsolete]
        public List<CustomRedirect> SearchCustomRedirects(string searchWord)
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            var customRedirects = from s in store.Items<CustomRedirect>()
                                  where s.NewUrl.Contains(searchWord) || s.OldUrl.Contains(searchWord)
                                  select s;

            return customRedirects.ToList();
        }
    }
}
