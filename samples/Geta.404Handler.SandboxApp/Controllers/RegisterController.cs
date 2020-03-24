using Geta._404Handler.SandboxApp.Models;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using EPiServer.Web.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Profile;
using EPiServer.Security;
using EPiServer.DataAbstraction;
using EPiServer.Personalization;

namespace Geta._404Handler.SandboxApp.Controllers
{
    /// <summary>
    /// Used to register a user for first time
    /// </summary>
    public class RegisterController : Controller
    {
        const string AdminRoleName = "WebAdmins";
        public const string ErrorKey = "CreateError";

        public ActionResult Index()
        {
            return View();
        }

        //
        // POST: /Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Index(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                UIUserCreateStatus status;
                IEnumerable<string> errors = Enumerable.Empty<string>();
                var result = UIUserProvider.CreateUser(model.Username, model.Password, model.Email, null, null, true, out status, out errors);
                if (status == UIUserCreateStatus.Success)
                {
                    UIRoleProvider.CreateRole(AdminRoleName);
                    UIRoleProvider.AddUserToRoles(result.Username, new string[] { AdminRoleName });

                    if (ProfileManager.Enabled)
                    {
                        var profile = EPiServerProfile.Wrap(ProfileBase.Create(result.Username));
                        profile.Email = model.Email;
                        profile.Save();
                    }

                    AdministratorRegistrationPage.IsEnabled = false;
                    SetFullAccessToWebAdmin();
                    var resFromSignIn = UISignInManager.SignIn(UIUserProvider.Name, model.Username, model.Password);
                    if (resFromSignIn)
                    {
                        return Redirect(UrlResolver.Current.GetUrl(ContentReference.StartPage));
                    }
                }
                AddErrors(errors);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private void SetFullAccessToWebAdmin()
        {
            var securityrep = ServiceLocator.Current.GetInstance<IContentSecurityRepository>();
            var permissions = securityrep.Get(ContentReference.RootPage).CreateWritableClone() as IContentSecurityDescriptor;
            permissions.AddEntry(new AccessControlEntry(AdminRoleName, AccessLevel.FullAccess));
            securityrep.Save(ContentReference.RootPage, permissions, SecuritySaveType.Replace);
        }

        private void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(ErrorKey, error);
            }
        }

        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!AdministratorRegistrationPage.IsEnabled)
            {
                filterContext.Result = new HttpNotFoundResult();
                return;
            }
            base.OnAuthorization(filterContext);
        }

        UIUserProvider UIUserProvider
        {
            get
            {
                return ServiceLocator.Current.GetInstance<UIUserProvider>();
            }
        }
        UIRoleProvider UIRoleProvider
        {
            get
            {
                return ServiceLocator.Current.GetInstance<UIRoleProvider>();
            }
        }
        UISignInManager UISignInManager
        {
            get
            {
                return ServiceLocator.Current.GetInstance<UISignInManager>();
            }
        }

    }
}
