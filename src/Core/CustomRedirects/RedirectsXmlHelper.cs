using System.Web.Hosting;
using EPiServer.Web.Hosting;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    public static class RedirectsXmlHelper
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static VirtualFile GetCustomRedirectsFile(string virtualFilePath)
        {
            // Attempt to load it virtually
            VirtualFile file = HostingEnvironment.VirtualPathProvider.GetFile(virtualFilePath) as VirtualFile;
            if (file != null)
            {
                // GetFile does not seem to return null, even if the file does not exist.
                // As an extra check if the file exist or not, we try to parse it to an UnifiedFile
                UnifiedFile f = file as UnifiedFile;
                return f != null ? file : null;
            }
            return null;
        }
    }


}
