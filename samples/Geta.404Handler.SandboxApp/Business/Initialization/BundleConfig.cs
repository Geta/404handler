using System.Web.Optimization;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace Geta._404Handler.SandboxApp.Business.Initialization
{
    [InitializableModule]
    public class BundleConfig : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            if (context.HostType == HostType.WebApplication)
            {
                RegisterBundles(BundleTable.Bundles);
            }
        }

        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Static/js/jquery.js", //jquery.js can be removed and linked from CDN instead, we use a local one for demo purposes without internet connectionzz
                        "~/Static/js/bootstrap.js"));

            bundles.Add(new StyleBundle("~/bundles/css")
                .Include("~/Static/css/bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Static/css/bootstrap-responsive.css")
                .Include("~/Static/css/media.css")
                .Include("~/Static/css/style.css", new CssRewriteUrlTransform())
                .Include("~/Static/css/editmode.css"));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
