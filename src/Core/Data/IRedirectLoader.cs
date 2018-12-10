using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound.Core.Data
{
    public interface IRedirectLoader
    {
        CustomRedirect GetByOldUrl(string oldUrl);
    }
}