using System.Web.Optimization;

namespace SchedulerWebApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region Script Bundles
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/moment.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/dateTimePicker").Include(
                "~/Scripts/bootstrap-datetimepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                     "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/tagIt").Include(
                     "~/Scripts/tag-it.js"));

            bundles.Add(new ScriptBundle("~/bundles/fooTable").Include(
                "~/Scripts/footable-bootstrap.v3.0.1/footable.js"));

            bundles.Add(new ScriptBundle("~/bundles/dataTables").Include(
                "~/Scripts/DataTables/jquery.dataTables.js",
                "~/Scripts/DataTables/dataTables.responsive.js"));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
                "~/Scripts/App/customscript.js"));

            #endregion

            #region Style Bundles
            bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-datetimepicker.css"));

            bundles.Add(new StyleBundle("~/Content/tagIt").Include(
                "~/Content/jquery-ui-flick-themes.css",
                "~/Content/jquery.tagit.css"));

            bundles.Add(new StyleBundle("~/Content/dataTablesStyles").Include(
                "~/Content/DataTables/css/dataTables.bootstrap.css",
                "~/Content/DataTables/css/jquery.dataTables.css",
                "~/Content/DataTables/css/responsive.dataTables.css"
                ));

            bundles.Add(new StyleBundle("~/Content/customStyle").Include(
                "~/Content/Custom.css"
                ));

            #endregion
        }
    }
}