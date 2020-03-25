using System.Linq;
using System.Web.Mvc;

namespace Geta._404Handler.SandboxApp.Business.Rendering
{
    /// <summary>
    /// Extends the Razor view engine to include the folders ~/Views/Shared/Blocks/ and ~/Views/Shared/PagePartials/
    /// when looking for partial views.
    /// </summary>
    public class SiteViewEngine : RazorViewEngine
    {
        private static readonly string[] AdditionalPartialViewFormats = new[]
            {
                TemplateCoordinator.BlockFolder + "{0}.cshtml",
                TemplateCoordinator.PagePartialsFolder + "{0}.cshtml"
            };

        public SiteViewEngine()
        {
            PartialViewLocationFormats = PartialViewLocationFormats.Union(AdditionalPartialViewFormats).ToArray();
        }
    }
}
