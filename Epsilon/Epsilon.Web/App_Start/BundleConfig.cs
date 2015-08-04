using System.Web;
using System.Web.Optimization;

namespace Epsilon.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/bootstrap-datepicker/bootstrap-datepicker.css",
                        "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/prettify").Include(
                        "~/Content/prettify/prettify.css"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/lib/jquery/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/lib/jquery/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/lib/modernizr/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/lib/bootstrap/bootstrap.js",
                      "~/Scripts/lib/respond/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap-datepicker").Include(
                        "~/Scripts/lib/bootstrap-datepicker/bootstrap-datepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/linq").Include(
                        "~/Scripts/lib/linq/linq.js"));

            bundles.Add(new ScriptBundle("~/bundles/angularjs").Include(
                        "~/Scripts/lib/angularjs/angular.js",
                        "~/Scripts/lib/ng-infinite-scroll/ng-infinite-scroll.js"));

            bundles.Add(new ScriptBundle("~/bundles/ngapp").Include(
                        // Controllers
                        "~/Scripts/ngapp/controllers/SubmissionAddressSearchController.js",
                        // Filters
                        "~/Scripts/ngapp/filters/LocalDateTime.js",
                        "~/Scripts/ngapp/filters/StringFormat.js",
                        // Directives
                        "~/Scripts/ngapp/directives/AddressMap.js",
                        "~/Scripts/ngapp/directives/ClickOnce.js",
                        "~/Scripts/ngapp/directives/MyOutgoingVerificationsSummary.js",
                        "~/Scripts/ngapp/directives/MySubmissionsSummary.js",
                        "~/Scripts/ngapp/directives/MyTokenTransactions.js",
                        "~/Scripts/ngapp/directives/TokenBalanceBadge.js",
                        "~/Scripts/ngapp/directives/TokenRewardsSummary.js",
                        "~/Scripts/ngapp/app.js"));

            bundles.Add(new ScriptBundle("~/bundles/run_prettify").Include(
                        "~/Scripts/lib/prettify/run_prettify.js"));
        }
    }
}
