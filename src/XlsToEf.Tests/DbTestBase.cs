using System.Data.Entity;
using XlsToEfTests.Infrastructure;
using XlsToEfTests.Models;

namespace XlsToEfTests
{
    public class DbTestBase
    {
        protected DbContext GetDb()
        {
            return new XlsToEfDbContext("XlsToEfTestDatabase");
        }

        protected void PersistToDatabase(params object[] objects)
        {
            using (var db = GetDb())
            {
                foreach (var o in objects)
                {
                    db.Set(o.GetType()).Add(o);
                }

                db.SaveChanges();
            }
        }

    }
}