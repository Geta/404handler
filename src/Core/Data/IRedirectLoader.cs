using System.Collections.Generic;
using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound.Core.Data
{
    public interface IRedirectLoader
    {
        CustomRedirect GetByOldUrl(string oldUrl);
        IEnumerable<CustomRedirect> GetAll();
        IEnumerable<CustomRedirect> GetByState(RedirectState state);
        IEnumerable<CustomRedirect> Find(string searchText);
    }
}