using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.IO;
using EPiServer.Web.Hosting;

namespace BVNetwork.FileNotFound.Redirects
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

        public static Stream GetStaticXmlFile(string virtualFile)
        {
            // Attempt to load it virtually
            VirtualFile file = GetCustomRedirectsFile(virtualFile);
            if (file != null)
            {
                return file.Open();
            }
            else
            {
                string localPath = HttpContext.Current.Server.MapPath(virtualFile);

                // The file might exist - we need to handle that
                if (File.Exists(localPath))
                {
                    return File.Open(localPath, FileMode.Open);
                }
            }

            _log.DebugFormat("No static XML-file found on {0} with path {1}", Environment.MachineName, virtualFile);
            return null;
        }

    }
}
