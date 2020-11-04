// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

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

        public static HttpContextBase Redirect(this HttpContextBase context, string url, int redirectType)
        {
            context.Response.Clear();
            context.Response.TrySkipIisCustomErrors = true;
            if (redirectType == Constants.Temporary)
            {
                context.Response.Redirect(url, endResponse: false);
            }
            else
            {
                context.Response.RedirectPermanent(url, endResponse: false);
            }
            
            return context;
        }
    }
}
