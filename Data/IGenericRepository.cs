using System.Linq.Expressions;

namespace Hangman.Data
{
    public interface IGenericRepository<TEntity>
    {
        public  IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        public Task<TEntity> GetByID(object id);

        public  void InsertAsync(TEntity entity);

        public void Delete(object id);

        public void Delete(TEntity entityToDelete);

        public void Update(TEntity entityToUpdate);
    }
}
