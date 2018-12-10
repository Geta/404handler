using System.Collections.Generic;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectsService
    {
        IEnumerable<CustomRedirect> GetAll();
        IEnumerable<CustomRedirect> GetAllExcludingIgnored();
        IEnumerable<CustomRedirect> GetIgnored();
        IEnumerable<CustomRedirect> GetDeleted();
        void AddOrUpdate(CustomRedirect redirect);
    }
}