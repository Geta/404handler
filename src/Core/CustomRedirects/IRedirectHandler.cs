namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public interface IRedirectHandler
    {
        /// <summary>
        /// The collection of custom redirects
        /// </summary>
        CustomRedirectCollection CustomRedirects { get; }

        /// <summary>
        /// Save a collection of redirects, and call method to raise an event in order to clear cache on all servers.
        /// </summary>
        /// <param name="redirects"></param>
        void SaveCustomRedirects(CustomRedirectCollection redirects);
    }
}