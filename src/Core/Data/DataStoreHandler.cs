using System.Collections.Generic;
using System.Linq;
using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound.Core.Data
{
    public class DataStoreHandler
    {
        public enum State
        {
            Saved = 0,
            Suggestion = 1,
            Ignored = 2,
            Deleted
        };

        private const string OldUrlPropertyName = "OldUrl";

        public void SaveCustomRedirect(CustomRedirect currentCustomRedirect)
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            var match = store.Find<CustomRedirect>(OldUrlPropertyName, currentCustomRedirect.OldUrl.ToLower()).SingleOrDefault();

            //if there is a match, replace the value.
            if (match != null)
            {
                store.Save(currentCustomRedirect, match.Id);
            }
            else
            {
                store.Save(currentCustomRedirect);
            }
        }

        /// <summary>
        /// Returns a list of all CustomRedirect objects in the Dynamic Data Store.
        /// </summary>
        /// <returns></returns>
        public List<CustomRedirect> GetCustomRedirects(bool excludeIgnored)
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            IEnumerable<CustomRedirect> customRedirects;
            if (excludeIgnored)
            {
                customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                                  where s.State.Equals((int)State.Saved)
                                  select s;
            }
            else
            {
                customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                                  select s;
            }
            return customRedirects.ToList();
        }

        public List<CustomRedirect> GetIgnoredRedirect()
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            var customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                              where s.State.Equals(State.Ignored)
                              select s;
            return customRedirects.ToList();
        }

        public List<CustomRedirect> GetDeletedRedirect()
        {
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            var deletedRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                              where s.State.Equals(State.Deleted)
                              select s;
            return deletedRedirects.ToList();
        }

        public void UnignoreRedirect()
        {
        }

        /// <summary>
        /// Delete CustomObject object from Data Store that has given "OldUrl" property
        /// </summary>
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
        public void DeleteAllCustomRedirects()
        {
            // In order to avoid a database timeout, we delete the items one by one.
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            foreach (var redirect in GetCustomRedirects(false))
            {
                store.Delete(redirect);
            }
        }

        public int DeleteAllIgnoredRedirects()
        {
            // In order to avoid a database timeout, we delete the items one by one.
            var store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            var ignoredRedirects = GetIgnoredRedirect();
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
