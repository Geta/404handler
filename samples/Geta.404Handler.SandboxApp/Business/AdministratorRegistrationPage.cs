using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using Owin;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Geta._404Handler.SandboxApp
{
    public static class AdministratorRegistrationPage
    {
        private static Func<bool> _isLocalRequest = () => false;

        private static Lazy<bool> _isAnyUserRegistered = new Lazy<bool>(() => false);

        private static bool? _isEnabled = null;

        public static bool IsEnabled
        {
            get
            {
                if (_isEnabled.HasValue)
                {
                    return _isEnabled.Value;
                }

                var showUserRegistration = _isLocalRequest() && !_isAnyUserRegistered.Value;
                if (!showUserRegistration)
                {
                    _isEnabled = false;
                }

                return showUserRegistration;
            }
            set
            {
                _isEnabled = value;
            }
        }

        public static void UseAdministratorRegistrationPage(this IAppBuilder app, Func<bool> isLocalRequest)
        {
            _isLocalRequest = isLocalRequest;
            _isAnyUserRegistered = new Lazy<bool>(IsAnyUserRegistered);
            GlobalFilters.Filters.Add(new RegistrationActionFilterAttribute());
            if (isLocalRequest())
            {
                AddRoute();
            }
        }

        private static bool IsAnyUserRegistered()
        {
            var provider = ServiceLocator.Current.GetInstance<UIUserProvider>();
            int totalUsers = 0;
            var res = provider.GetAllUsers(0, 1, out totalUsers);
            return totalUsers > 0;
        }

        public class RegistrationActionFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                var registerUrl = VirtualPathUtility.ToAbsolute("~/Register");
                if (IsEnabled && !context.RequestContext.HttpContext.Request.Path.StartsWith(registerUrl))
                {
                    context.Result = new RedirectResult(registerUrl);
                }
            }
        }

        static void AddRoute()
        {
            var routeData = new RouteValueDictionary();
            routeData.Add("Controller", "Register");
            routeData.Add("action", "Index");
            routeData.Add("id", " UrlParameter.Optional");
            RouteTable.Routes.Add("Register", new Route("{controller}/{action}/{id}", routeData, new MvcRouteHandler()) { RouteExistingFiles = false });
        }
    }
}
