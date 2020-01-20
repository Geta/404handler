using System.Collections.Generic;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectsService
    {
        IEnumerable<CustomRedirect> GetAll();
        IEnumerable<CustomRedirect> GetSaved();
        IEnumerable<CustomRedirect> GetIgnored();
        IEnumerable<CustomRedirect> GetDeleted();
        IEnumerable<CustomRedirect> Search(string searchText);
        void AddOrUpdate(CustomRedirect redirect);
        void AddOrUpdate(IEnumerable<CustomRedirect> redirects);
        void DeleteByOldUrl(string oldUrl);
        int DeleteAll();
        int DeleteAllIgnored();
    }
}