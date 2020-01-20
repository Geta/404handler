namespace BVNetwork.NotFound.Core.Data
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        void Save(TEntity entity);
        void Delete(TEntity entity);
    }
}