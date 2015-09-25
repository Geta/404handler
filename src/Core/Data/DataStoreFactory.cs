using System;

namespace BVNetwork.NotFound.Core.Data
{
    public static class DataStoreFactory
    {
        public static EPiServer.Data.Dynamic.DynamicDataStore GetStore(Type t)
        {
            // GetStore will only return null the first time this method is called for a Type
            // In that case the ?? C# operator will call CreateStore
            // EPiServer.Data.Dynamic.DynamicDataStoreFactory.Instance.DeleteStore(t,true);
            return EPiServer.Data.Dynamic.DynamicDataStoreFactory.Instance.GetStore(t) ??
                EPiServer.Data.Dynamic.DynamicDataStoreFactory.Instance.CreateStore(t);
        }
    }
}
