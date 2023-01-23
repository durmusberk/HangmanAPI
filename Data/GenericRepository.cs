using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;


namespace Hangman.Data
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class 
    {
        internal readonly ApplicationDbContext _db;
        internal readonly DbSet<TEntity> _dbset;

        public GenericRepository(ApplicationDbContext db)
        {
            _db = db;
            _dbset = db.Set<TEntity>();
        }

        public  virtual IEnumerable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbset;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public async virtual Task<TEntity> GetByID(object id)
        {
            return await _dbset.FindAsync(id);
        }

        public async virtual void InsertAsync(TEntity entity)
        {
            await _dbset.AddAsync(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity? entityToDelete = _dbset.Find(id);
            if (entityToDelete is not null)
            {
                Delete(entityToDelete);
            }
            
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_db.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbset.Attach(entityToDelete);
            }
            _dbset.Remove(entityToDelete);
        }

        public virtual void Delete(List<TEntity> entities)
        {
            foreach (var entity in entities) 
            {
                Delete(entity);
            }
        }


        public  virtual void Update(TEntity entityToUpdate)
        {
            _dbset.Attach(entityToUpdate);
            _db.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}