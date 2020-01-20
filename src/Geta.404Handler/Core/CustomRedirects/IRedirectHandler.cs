using System;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectHandler
    {
        /// <summary>
        /// Returns custom redirect for the not found url
        /// </summary>
        /// <param name="urlNotFound"></param>
        /// <returns></returns>
        CustomRedirect Find(Uri urlNotFound);
    }
}