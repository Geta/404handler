using System.Web.Mvc;
using Geta._404Handler.SandboxApp.Business;
using Geta._404Handler.SandboxApp.Models.Pages;
using Geta._404Handler.SandboxApp.Models.ViewModels;
using EPiServer.Web.Mvc;
using EPiServer.Shell.Security;

namespace Geta._404Handler.SandboxApp.Controllers
{
    /// <summary>
    /// All controllers that renders pages should inherit from this class so that we can
    /// apply action filters, such as for output caching site wide, should we want to.
    /// </summary>
    public abstract class PageControllerBase<T> : PageController<T>, IModifyLayout
        where T : SitePageData
    {

        protected EPiServer.ServiceLocation.Injected<UISignInManager> UISignInManager;

        /// <summary>
        /// Signs out the current user and redirects to the Index action of the same controller.
        /// </summary>
        /// <remarks>
        /// There's a log out link in the footer which should redirect the user to the same page.
        /// As we don't have a specific user/account/login controller but rely on the login URL for
        /// forms authentication for login functionality we add an action for logging out to all
        /// controllers inheriting from this class.
        /// </remarks>
        public ActionResult Logout()
        {
            UISignInManager.Service.SignOut();
            return RedirectToAction("Index");
        }

        public virtual void ModifyLayout(LayoutModel layoutModel)
        {
            var page = PageContext.Page as SitePageData;
            if(page != null)
            {
                layoutModel.HideHeader = page.HideSiteHeader;
                layoutModel.HideFooter = page.HideSiteFooter;
            }
        }
    }
}
