using System.Reflection;
using System.Web.Mvc;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    public class NotFoundPageAttribute : ActionFilterAttribute
    {
        private static readonly ILogger Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            Log.Debug("Starting 404 action filter");
            filterContext.ParentActionViewContext.ViewBag.Referrer = NotFoundPageUtil.GetReferer(request);
            filterContext.ParentActionViewContext.ViewBag.NotFoundUrl = NotFoundPageUtil.GetUrlNotFound(request);
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            filterContext.HttpContext.Response.StatusCode = NotFoundPageUtil.GetStatusCode(request);
            filterContext.HttpContext.Response.Status = "404 File not found";
        }
    }
}