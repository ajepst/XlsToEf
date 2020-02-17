using System.Data.Entity;
using XlsToEf.Tests.Infrastructure;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests
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