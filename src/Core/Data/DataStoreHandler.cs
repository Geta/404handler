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
            var redirects = excludeIgnored ? _redirectLoader.GetByState(RedirectState.Saved) : _redirectLoader.GetAll();
            return redirects.ToList();
        }

        [Obsolete]
        public List<CustomRedirect> GetIgnoredRedirect()
        {
            var customRedirects = _redirectLoader.GetByState(RedirectState.Ignored);
            return customRedirects.ToList();
        }

        [Obsolete]
        public List<CustomRedirect> GetDeletedRedirect()
        {
            var deletedRedirects = _redirectLoader.GetByState(RedirectState.Deleted);
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

            var match = _redirectLoader.GetByOldUrl(oldUrl);
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
            var redirects = GetAll().ToList();
            foreach (var redirect in redirects)
            {
                _repository.Delete(redirect);
            }
            return redirects.Count;
        }

        [Obsolete]
        public int DeleteAllIgnoredRedirects()
        {
            // In order to avoid a database timeout, we delete the items one by one.
            var ignoredRedirects = GetIgnored().ToList();
            foreach (var redirect in ignoredRedirects)
            {
                _repository.Delete(redirect);
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
            var customRedirects = _redirectLoader.Find(searchWord);
            return customRedirects.ToList();
        }
    }
}
