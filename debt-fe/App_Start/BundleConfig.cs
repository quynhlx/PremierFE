using System.Web;
using System.Web.Optimization;

namespace debt_fe
{
    public class BundleConfig
    {        
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",                      
                      "~/Scripts/respond.js"));

            //
            // js plugins
            // 
            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                "~/Scripts/plugins/jquery.dataTables.js",
                "~/Scripts/plugins/bootstrapValidator.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/css/font-awesome.css",
                      "~/Content/extensions/box-model.css",
                      "~/Content/styles.css"));

            //
            // css extensions
            //
            bundles.Add(new StyleBundle("~/Content/extensions").Include(
                "~/Content/extensions/jquery.dataTables.css",
                "~/Content/extensions/bootstrapValidator.css"));

            bundles.Add(new StyleBundle("~/Content/custom").Include("~/Content/site.css"));
        }
    }
}
