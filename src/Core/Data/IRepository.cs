using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound.Core.Data
{
    public interface IRepository<TEntity, TId>
        where TEntity : class
    {
        void Save(CustomRedirect customRedirect);
        void Delete(CustomRedirect customRedirect);
    }
}