using System.Web;

namespace BVNetwork.NotFound.Core.Web
{
    public static class HttpContextBaseExtensions
    {
        public static HttpContextBase ClearServerError(this HttpContextBase context)
        {
            context.Server.ClearError();
            return context;
        }

        public static HttpContextBase SetStatusCode(this HttpContextBase context, int statusCode)
        {
            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.StatusCode = statusCode;
            return context;
        }

        public static HttpContextBase RedirectPermanent(this HttpContextBase context, string url)
        {
            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.RedirectPermanent(url);
            return context;
        }

        public static HttpContextBase End(this HttpContextBase context)
        {
            context.Response.End();
            return context;
        }
    }
}