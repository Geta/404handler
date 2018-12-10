using BVNetwork.NotFound.Core.CustomRedirects;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace BVNetwork.NotFound.Core.Data
{
    public class DdsRedirectRepository : IRepository<CustomRedirect, Identity>
    {
        private DynamicDataStore Store => DataStoreFactory.GetStore(typeof(CustomRedirect));

        public void Save(CustomRedirect customRedirect)
        {
            Store.Save(customRedirect);
        }
    }
}