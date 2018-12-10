using System.Collections.Generic;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectsService
    {
        IEnumerable<CustomRedirect> GetAll();
        IEnumerable<CustomRedirect> GetAllExcludingIgnored();
        IEnumerable<CustomRedirect> GetIgnored();
        IEnumerable<CustomRedirect> GetDeleted();
        IEnumerable<CustomRedirect> Search(string searchText);
        void AddOrUpdate(CustomRedirect redirect);
        void DeleteByOldUrl(string oldUrl);
        int DeleteAll();
        int DeleteAllIgnored();
    }
}