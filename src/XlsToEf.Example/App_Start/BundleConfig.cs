using System.Web;
using System.Web.Optimization;

namespace XlsToEf.Example
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery.ui.widget.js",
                "~/Scripts/jquery.fileupload.js",
                "~/Scripts/jquery.fileupload-process.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/toastr").Include(
                "~/Scripts/toastr.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));



            bundles.Add(new StyleBundle("~/Content/css/all-styles").Include(
                "~/Content/site.css",
                "~/Content/bootstrap.css",
                "~/Content/toastr.css")
                );

            bundles.Add(new ScriptBundle("~/bundles/xlstoef").Include(
                "~/Scripts/app.js",
                "~/Scripts/import.js"));
        }
    }
}
