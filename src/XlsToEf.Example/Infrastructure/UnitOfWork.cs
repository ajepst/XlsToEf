using System.Web.Mvc;

namespace XlsToEf.Example.Infrastructure
{
    public class UnitOfWork : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var directory = DependencyResolver.Current.GetService<XlsToEfDbContext>();
            directory.BeginTransaction();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var directory = DependencyResolver.Current.GetService<XlsToEfDbContext>();
            directory.CloseTransaction(filterContext.Exception);
        }
    }
}