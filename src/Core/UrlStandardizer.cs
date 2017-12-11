using System;
using EPiServer.ServiceLocation;

namespace BVNetwork.NotFound.Core
{
    public class UrlStandardizer
    {
        public static Func<IUrlStandardizer> Accessor { get; set; } = () => ServiceLocator.Current.GetInstance<IUrlStandardizer>();

        private static IUrlStandardizer Standardizer => Accessor();

        public static string Standardize(string url)
        {
            return Standardizer.Standardize(url);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IUrlStandardizer
    {
        /*[CanBeNull]*/
        string Standardize(/*[CanBeNull]*/ string url);
    }

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

    public class ToLowerUrlStandardizer : IUrlStandardizer
    {
        public virtual string Standardize(string url)
        {
            return url?.ToLower();
        }
    }
}