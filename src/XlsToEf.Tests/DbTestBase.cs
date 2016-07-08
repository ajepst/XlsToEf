using System.Data.Entity;
using XlsToEf.Tests.Infrastructure;
using XlsToEf.Tests.Models;

namespace XlsToEf.Tests
{
    public class DbTestBase
    {
        protected DbContext GetDb()
        {
            return new XlsToEfDbContext("XlsToEf");
        }

        protected void PersistToUnitDatabase(params object[] objects)
        {
            using (var udb = GetDb())
            {
                foreach (var o in objects)
                {
                    udb.Set(o.GetType()).Add(o);
                }

                udb.SaveChanges();
            }
        }

    }
}