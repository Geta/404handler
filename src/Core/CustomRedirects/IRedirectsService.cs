using System.Collections.Generic;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectsService
    {
        IEnumerable<CustomRedirect> GetAll();
        IEnumerable<CustomRedirect> GetAllExcludingIgnored();
        void AddOrUpdate(CustomRedirect redirect);
    }
}