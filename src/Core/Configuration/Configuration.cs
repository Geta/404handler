using System;
using System.Configuration;
using BVNetwork.NotFound.Configuration;

namespace BVNetwork.NotFound.Core.Configuration
{
    public enum FileNotFoundMode
    {
        /// <summary>
        /// 
        /// </summary>
        On,
        /// <summary>
        /// 
        /// </summary>
        Off,
        /// <summary>
        /// 
        /// </summary>
        RemoteOnly
    };

    public enum LoggerMode
    {
        On, Off
    };


    /// <summary>
    /// Configuration utility class for the custom 404 handler
    /// </summary>
    public class Configuration
    {
        private const string DEF_REDIRECTS_XML_FILE = "~/CustomRedirects.config";
        private const string DEF_NOTFOUND_PAGE = "~/bvn/filenotfound/notfound.aspx";
        private const LoggerMode DEF_LOGGING = LoggerMode.On;
        private static LoggerMode _logging = DEF_LOGGING;
        private const int DEF_BUFFER_SIZE = 30;
        private const int DEF_THRESHHOLD = 5;
        private const string KEY_ERROR_FALLBACK = "EPfBVN404UseStdErrorHandlerAsFallback";
        private const FileNotFoundMode DEF_NOTFOUND_MODE = FileNotFoundMode.On;
        private static FileNotFoundMode _handlerMode = DEF_NOTFOUND_MODE;
        private static bool _handlerMode_IsRead = false;
        private static bool _fallbackToEPiServerError = false;
        private static bool _fallbackToEPiServerError_IsRead = false;

        public const int CURRENT_VERSION = 3;



        // Only contains static methods for reading configuration values
        // Should not be instanciable
        private Configuration()
        {
        }


        /// <summary>
        /// Tells the errorhandler to use EPiServer Exception Manager
        /// to render unhandled errors. Defaults to False.
        /// </summary>
        public static bool FallbackToEPiServerErrorExceptionManager
        {
            get
            {
                if (_fallbackToEPiServerError_IsRead == false)
                {
                    _fallbackToEPiServerError_IsRead = true;
                    if (ConfigurationManager.AppSettings[KEY_ERROR_FALLBACK] != null)
                        bool.TryParse(ConfigurationManager.AppSettings[KEY_ERROR_FALLBACK], out _fallbackToEPiServerError);
                }
                return _fallbackToEPiServerError;
            }
        }

        /// <summary>
        /// The mode to use for the 404 handler
        /// </summary>
        public static FileNotFoundMode FileNotFoundHandlerMode
        {
            get
            {
                if (_handlerMode_IsRead == false)
                {
                    string mode = Bvn404HandlerConfiguration.Instance.HandlerMode;
                    if (mode == null)
                        mode = DEF_NOTFOUND_MODE.ToString();

                    try
                    {
                        _handlerMode = (FileNotFoundMode)Enum.Parse(typeof(FileNotFoundMode), mode, true /* Ignores case */);
                    }
                    catch
                    {
                        _handlerMode = DEF_NOTFOUND_MODE;
                    }
                    _handlerMode_IsRead = true;
                }

                return _handlerMode;
            }
        }

        /// <summary>
        /// The mode to use for the 404 handler
        /// </summary>
        public static LoggerMode Logging
        {
            get
            {
                string mode = BVNetwork.NotFound.Configuration.Bvn404HandlerConfiguration.Instance.Logging;
                if (mode == null)
                    mode = DEF_LOGGING.ToString();

                try
                {
                    _logging = (LoggerMode)Enum.Parse(typeof(LoggerMode), mode, true /* Ignores case */);
                }
                catch
                {
                    _logging = DEF_LOGGING;
                }


                return _logging;
            }
        }


        /// <summary>
        /// The virtual path to the 404 handler aspx file.
        /// </summary>
        public static string FileNotFoundHandlerPage
        {
            get
            {

                if (Bvn404HandlerConfiguration.Instance.FileNotFoundPage == null ||
                   Bvn404HandlerConfiguration.Instance.FileNotFoundPage == string.Empty)
                {
                    return DEF_NOTFOUND_PAGE;
                }

                return Bvn404HandlerConfiguration.Instance.FileNotFoundPage;
            }
        }

        /// <summary>
        /// The relative path to the custom redirects xml file, including the name of the
        /// xml file. The 404 handler will map the result to a server path.
        /// </summary>
        public static string CustomRedirectsXmlFile
        {
            get
            {
                if (Bvn404HandlerConfiguration.Instance != null &&
                    string.IsNullOrEmpty(Bvn404HandlerConfiguration.Instance.RedirectsXmlFile) == false)
                {
                    return Bvn404HandlerConfiguration.Instance.RedirectsXmlFile;
                }

                return DEF_REDIRECTS_XML_FILE;
            }
        }


        /// <summary>
        /// BufferSize for logging of redirects. 
        /// </summary>
        public static int BufferSize
        {
            get
            {
                if (Bvn404HandlerConfiguration.Instance != null && Bvn404HandlerConfiguration.Instance.BufferSize != -1)
                {
                    return Bvn404HandlerConfiguration.Instance.BufferSize;
                }   

                return DEF_BUFFER_SIZE;
            }
        }

        /// <summary>
        /// ThreshHold value for redirect logging.
        /// </summary>
        public static int ThreshHold
        {
            get
            {
                if (Bvn404HandlerConfiguration.Instance != null && Bvn404HandlerConfiguration.Instance.Threshold != -1)
                {
                    return Bvn404HandlerConfiguration.Instance.Threshold;
                }

                return DEF_THRESHHOLD;
            }
        }

    }
}
