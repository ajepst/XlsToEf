using System;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq.Expressions;

namespace XlsToEf.Import
{

    public interface IBulkBaseDbContext
    {
        Task InsertBulk();
        Task UpsertBulk();
        Task UpdateBulk();
        Task<int> SaveChangesAsync();
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entityToUpdate) where TEntity : class;
        Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> expr) where TEntity : class;
        Task<TEntity> FindAsync<TEntity>(object idData) where TEntity : class;
        TEntity Add<TEntity>(TEntity entity) where TEntity : class;
        DbContext InnerContext {get;}
    }

    public class NoBulkImplementation : IBulkBaseDbContext
    {
        DbContext _context;

        public NoBulkImplementation(DbContext context)
        {
            _context = context;
        }

        public DbContext InnerContext
        {
            get { return _context; }
        }

        public Task InsertBulk()
        {
            throw new Exception("Default implementation does not have bulk operations available.");
        }

        public Task UpdateBulk()
        {
            throw new Exception("Default implementation does not have bulk operations available.");
        }

        public Task UpsertBulk()
        {
            throw new Exception("Default implementation does not have bulk operations available.");
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entityToUpdate) where TEntity : class
        {
            return _context.Entry(entityToUpdate);
        }

        public async Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> expr) where TEntity : class
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(expr);
        }

        public async Task<TEntity> FindAsync<TEntity>(object idData) where TEntity : class
        {
            return await _context.Set<TEntity>().FindAsync(idData);
        }

        public TEntity Add<TEntity>(TEntity entity) where TEntity : class
        {
            return _context.Set<TEntity>().Add(entity);
        } 
    }
}
