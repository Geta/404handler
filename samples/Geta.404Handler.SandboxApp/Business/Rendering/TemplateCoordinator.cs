using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Geta._404Handler.SandboxApp.Controllers;
using Geta._404Handler.SandboxApp.Models.Blocks;
using Geta._404Handler.SandboxApp.Models.Pages;
using EPiServer.Web;
using EPiServer.Web.Mvc;

namespace Geta._404Handler.SandboxApp.Business.Rendering
{
    [ServiceConfiguration(typeof(IViewTemplateModelRegistrator))]
    public class TemplateCoordinator : IViewTemplateModelRegistrator
    {
        public const string BlockFolder = "~/Views/Shared/Blocks/";
        public const string PagePartialsFolder = "~/Views/Shared/PagePartials/";

        public static void OnTemplateResolved(object sender, TemplateResolverEventArgs args)
        {
            //Disable DefaultPageController for page types that shouldn't have any renderer as pages
            if (args.ItemToRender is IContainerPage && args.SelectedTemplate != null && args.SelectedTemplate.TemplateType == typeof(DefaultPageController))
            {
                args.SelectedTemplate = null;
            }
        }

        /// <summary>
        /// Registers renderers/templates which are not automatically discovered,
        /// i.e. partial views whose names does not match a content type's name.
        /// </summary>
        /// <remarks>
        /// Using only partial views instead of controllers for blocks and page partials
        /// has performance benefits as they will only require calls to RenderPartial instead of
        /// RenderAction for controllers.
        /// Registering partial views as templates this way also enables specifying tags and
        /// that a template supports all types inheriting from the content type/model type.
        /// </remarks>
        public void Register(TemplateModelCollection viewTemplateModelRegistrator)
        {
            viewTemplateModelRegistrator.Add(typeof(JumbotronBlock), new TemplateModel
            {
                Tags = new[] { Global.ContentAreaTags.FullWidth },
                AvailableWithoutTag = false,
                Path = BlockPath("JumbotronBlockWide.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(TeaserBlock), new TemplateModel
            {
                Name = "TeaserBlockWide",
                Tags = new[] { Global.ContentAreaTags.TwoThirdsWidth, Global.ContentAreaTags.FullWidth },
                AvailableWithoutTag = false,
                Path = BlockPath("TeaserBlockWide.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(SitePageData), new TemplateModel
            {
                Name = "PagePartial",
                Inherit = true,
                AvailableWithoutTag = true,
                Path = PagePartialPath("Page.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(SitePageData), new TemplateModel
            {
                Name = "PagePartialWide",
                Inherit = true,
                Tags = new[] { Global.ContentAreaTags.TwoThirdsWidth, Global.ContentAreaTags.FullWidth },
                AvailableWithoutTag = false,
                Path = PagePartialPath("PageWide.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(ContactPage), new TemplateModel
            {
                Name = "ContactPagePartialWide",
                Tags = new[] { Global.ContentAreaTags.TwoThirdsWidth, Global.ContentAreaTags.FullWidth },
                AvailableWithoutTag = false,
                Path = PagePartialPath("ContactPageWide.cshtml")
            });

            viewTemplateModelRegistrator.Add(typeof(IContentData), new TemplateModel
            {
                Name = "NoRendererMessage",
                Inherit = true,
                Tags = new[] { Global.ContentAreaTags.NoRenderer },
                AvailableWithoutTag = false,
                Path = BlockPath("NoRenderer.cshtml")
            });
        }

        private static string BlockPath(string fileName)
        {
            return string.Format("{0}{1}", BlockFolder, fileName);
        }

        private static string PagePartialPath(string fileName)
        {
            return string.Format("{0}{1}", PagePartialsFolder, fileName);
        }
    }
}
