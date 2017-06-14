using System;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace XlsToEf.Import
{

    public interface IBulkBaseDbContext 
    {
        Task InsertBulk();
        Task UpsertBulk();
        Task UpdateBulk();
        Task<int> SaveChangesAsync();
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entityToUpdate) where TEntity : class;

    }

    public class NoBulkImplementation : IBulkBaseDbContext
    {
        DbContext _context;

        public NoBulkImplementation(DbContext context)
        {
            _context = context;
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

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>();
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entityToUpdate) where TEntity : class
        {
            return _context.Entry(entityToUpdate);
        }
    }
}
