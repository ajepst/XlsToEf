using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(XlsToEf.Example.Startup))]
namespace XlsToEf.Example
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}
