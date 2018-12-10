using System.Linq;
using BVNetwork.NotFound.Core.CustomRedirects;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace BVNetwork.NotFound.Core.Data
{
    public class DdsRedirectRepository : IRepository<CustomRedirect, Identity>, IRedirectLoader
    {
        private DynamicDataStore Store => DataStoreFactory.GetStore(typeof(CustomRedirect));
        private const string OldUrlPropertyName = "OldUrl";

        public void Save(CustomRedirect customRedirect)
        {
            Store.Save(customRedirect);
        }

        public CustomRedirect GetByOldUrl(string oldUrl)
        {
            return Store.Find<CustomRedirect>(OldUrlPropertyName, oldUrl.ToLower()).SingleOrDefault();
        }
    }
}