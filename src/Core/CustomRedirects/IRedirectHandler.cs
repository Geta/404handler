using System;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectHandler
    {
        /// <summary>
        /// Returns custom redirect for the not found url
        /// </summary>
        /// <param name="urlNotFound"></param>
        /// <param name="referrer"></param>
        /// <returns></returns>
        CustomRedirect Find(Uri urlNotFound, Uri referrer);

        /// <summary>
        /// Save a collection of redirects, and call method to raise an event in order to clear cache on all servers.
        /// </summary>
        /// <param name="redirects"></param>
        void SaveCustomRedirects(CustomRedirectCollection redirects);
    }
}