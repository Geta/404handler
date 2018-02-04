using System;
using EPiServer.ServiceLocation;

namespace BVNetwork.NotFound.Core
{
    /// <summary>
    /// Provides an access to the current IUrlStandardizer inplementation
    /// </summary>
    public class UrlStandardizer
    {
        public static Func<IUrlStandardizer> Accessor { get; set; } = () => ServiceLocator.Current.GetInstance<IUrlStandardizer>();

        private static IUrlStandardizer Standardizer => Accessor();

        /// <summary>
        /// Standardizes urls 
        /// </summary>
        /// <param name="url">url</param>
        /// <returns>Returns a standard representation of the url</returns>
        public static string Standardize(string url)
        {
            return Standardizer.Standardize(url);
        }
    }

    /// <summary>
    /// An algorithm which makes urls standard.
    /// For example for browser http://MySite.com and http://mysite.com/ are equal,
    /// so this interface defines a standard url as an OldUrl in a redirect rule.  
    /// </summary>
    public interface IUrlStandardizer
    {
        /// <summary>
        /// Standardizes urls 
        /// </summary>
        /// <param name="url">url</param>
        /// <returns>Returns a standard representation of the given url</returns>
        /*[CanBeNull]*/
        string Standardize(/*[CanBeNull]*/ string url);
    }

    /// <summary>
    /// Urls are lowercased, plus the trailing / is ignored (when no query string present in url)
    /// http://MySite.com and http://mysite.com/ are both represented by a standard url http://mysite.com
    /// </summary>
    [ServiceConfiguration(typeof(IUrlStandardizer), Lifecycle = ServiceInstanceScope.Singleton)]
    public class DefaultUrlStandardizer : IUrlStandardizer
    {
        public virtual string Standardize(string url)
        {
            if (url == null)
                return null;
            var result = (url.EndsWith("/") && !url.Contains("?") ? url.Substring(0, url.Length - 1) : url).ToLower();
            return result;
        }
    }

    /// <summary>
    /// The original BVNHandler behavior - all urls are lowercased
    /// </summary>
    public class ToLowerUrlStandardizer : IUrlStandardizer
    {
        public virtual string Standardize(string url)
        {
            return url?.ToLower();
        }
    }
}