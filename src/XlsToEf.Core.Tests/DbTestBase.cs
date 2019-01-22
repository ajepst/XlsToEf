using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XlsToEf.Core.Tests.Infrastructure;

namespace XlsToEf.Core.Tests
{
    public class DbTestBase
    {
        protected DbContext GetDb()
        {
            var optionsBuilder = new DbContextOptionsBuilder<XlsToEfDbContext>();
            optionsBuilder.UseSqlServer("XlsToEfTestDatabase");
            return new XlsToEfDbContext(optionsBuilder.Options);
        }

        protected async Task PersistToDatabase<T>(T entity)
        where T: class
        {
            using (var db = GetDb())
            {

                await db.Set<T>().AddAsync(entity);

                await db.SaveChangesAsync();
            }
        }

        protected async Task PersistToDatabase<T, Q>(T entity, Q entity2 )
            where T : class
            where Q : class
        {
            using (var db = GetDb())
            {

                await db.Set<T>().AddAsync(entity);
                await db.Set<Q>().AddAsync(entity2);

                await db.SaveChangesAsync();
            }
        }

    }
}