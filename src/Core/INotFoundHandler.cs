namespace BVNetwork.NotFound.Core
{
    /// <summary>
    /// Interface for creating custom redirect handling.
    /// </summary>
    public interface INotFoundHandler
    {
        /// <summary>
        /// Create a redirect url from the old url.
        /// This could for example be done by using Regex.Replace(...)
        /// </summary>
        /// <param name="url">The old url which will be redirected</param>
        /// <returns>The new url for the redirect. If no new url has been created, null should be returned instead.</returns>
        string RewriteUrl(string url);
    }
}
