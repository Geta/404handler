using System;
using System.Collections.Generic;
using System.Linq;
using BVNetwork.NotFound.Core.CustomRedirects;
using EPiServer.ServiceLocation;

namespace BVNetwork.NotFound.Core.Data
{
    [Obsolete]
    public class DataStoreHandler
    {
        private readonly IRedirectsService _redirectsService;

        public DataStoreHandler()
        {
            _redirectsService = ServiceLocator.Current.GetInstance<DefaultRedirectsService>();
        }

        [Obsolete]
        public void SaveCustomRedirect(CustomRedirect currentCustomRedirect)
        {
            _redirectsService.AddOrUpdate(currentCustomRedirect);
        }

        /// <summary>
        /// Returns a list of all CustomRedirect objects in the Dynamic Data Store.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public List<CustomRedirect> GetCustomRedirects(bool excludeIgnored)
        {
            return excludeIgnored
                ? _redirectsService.GetAllExcludingIgnored().ToList()
                : _redirectsService.GetAll().ToList();

        }

        [Obsolete]
        public List<CustomRedirect> GetIgnoredRedirect()
        {
            return _redirectsService.GetIgnored().ToList();

        }

        [Obsolete]
        public List<CustomRedirect> GetDeletedRedirect()
        {
            return _redirectsService.GetDeleted().ToList();
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
            _redirectsService.DeleteByOldUrl(oldUrl);
        }

        /// <summary>
        /// Delete all CustomRedirect objects
        /// </summary>
        [Obsolete]
        public void DeleteAllCustomRedirects()
        {
            _redirectsService.DeleteAll();
        }

        [Obsolete]
        public int DeleteAllIgnoredRedirects()
        {
            return _redirectsService.DeleteAllIgnored();
        }

        /// <summary>
        /// Find all CustomRedirect objects which has a OldUrl og NewUrl that contains the search word.
        /// </summary>
        /// <param name="searchWord"></param>
        /// <returns></returns>
        [Obsolete]
        public List<CustomRedirect> SearchCustomRedirects(string searchWord)
        {
            return _redirectsService.Search(searchWord).ToList();
        }
    }
}
