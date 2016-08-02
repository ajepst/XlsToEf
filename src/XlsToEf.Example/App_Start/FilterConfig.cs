using System.Web;
using System.Web.Mvc;
using XlsToEf.Example.Infrastructure;

namespace XlsToEf.Example
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new UnitOfWork());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
