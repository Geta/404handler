using System.Web.Mvc;
using Geta._404Handler.SandboxApp.Models.Pages;
using Geta._404Handler.SandboxApp.Models.ViewModels;
using EPiServer.Web.Routing;

namespace Geta._404Handler.SandboxApp.Business
{
    /// <summary>
    /// Intercepts actions with view models of type IPageViewModel and populates the view models
    /// Layout and Section properties.
    /// </summary>
    /// <remarks>
    /// This filter frees controllers for pages from having to care about common context needed by layouts
    /// and other page framework components allowing the controllers to focus on the specifics for the page types
    /// and actions that they handle.
    /// </remarks>
    public class PageContextActionFilter : IResultFilter
    {
        private readonly PageViewContextFactory _contextFactory;
        public PageContextActionFilter(PageViewContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var viewModel = filterContext.Controller.ViewData.Model;

            var model = viewModel as IPageViewModel<SitePageData>;
            if (model != null)
            {
                var currentContentLink = filterContext.RequestContext.GetContentLink();

                var layoutModel = model.Layout ?? _contextFactory.CreateLayoutModel(currentContentLink, filterContext.RequestContext);

                var layoutController = filterContext.Controller as IModifyLayout;
                if(layoutController != null)
                {
                    layoutController.ModifyLayout(layoutModel);
                }

                model.Layout = layoutModel;

                if (model.Section == null)
                {
                    model.Section = _contextFactory.GetSection(currentContentLink);
                }
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
