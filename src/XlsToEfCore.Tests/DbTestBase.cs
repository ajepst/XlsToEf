using System.Configuration;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using XlsToEfCore.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace XlsToEfCore.Tests
{
    public class DbTestBase
    {
        protected DbContext GetDb()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            var optionsBuilder = new DbContextOptionsBuilder<XlsToEfDbContext>();
            optionsBuilder.UseSqlServer(configBuilder.Build().GetConnectionString( "XlsToEfTestDatabase"));
            return new XlsToEfDbContext(optionsBuilder.Options);
        }

        protected async Task PersistToDatabase<T>(params T[] entity)
        where T: class
        {
            using (var db = GetDb())
            {
                foreach (var e in entity)
                    await db.Set<T>().AddAsync(e);

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