using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using BVNetwork.NotFound.Configuration;

namespace BVNetwork.NotFound.Core.Configuration
{
    /// <summary>
    /// Configuration utility class for the custom 404 handler
    /// </summary>
    public class Configuration : IConfiguration
    {
        private const string DefRedirectsXmlFile = "~/CustomRedirects.config";
        private const string DefNotfoundPage = "~/bvn/filenotfound/notfound.aspx";
        private const LoggerMode DefLogging = LoggerMode.On;
        private static LoggerMode _logging = DefLogging;
        private const int DefBufferSize = 30;
        private const int DefThreshhold = 5;
        private const string KeyErrorFallback = "EPfBVN404UseStdErrorHandlerAsFallback";
        private const FileNotFoundMode DefNotfoundMode = FileNotFoundMode.On;
        private static FileNotFoundMode _handlerMode = DefNotfoundMode;
        private static bool _handlerModeIsRead;
        private static bool _fallbackToEPiServerError;
        private static bool _fallbackToEPiServerErrorIsRead;
        private const string DefIgnoredExtensions = "jpg,gif,png,css,js,ico,swf,woff";
        private static List<string> _ignoredResourceExtensions;
        private static bool? _logWithHostname;

        public const int CurrentVersion = 3;


        // Only contains static methods for reading configuration values
        // Should not be instanciable
        private Configuration()
        {
        }

        public static Configuration Instance { get; } = new Configuration();

        /// <summary>
        /// Tells the errorhandler to use EPiServer Exception Manager
        /// to render unhandled errors. Defaults to False.
        /// </summary>
        public bool FallbackToEPiServerErrorExceptionManager
        {
            get
            {
                if (_fallbackToEPiServerErrorIsRead == false)
                {
                    _fallbackToEPiServerErrorIsRead = true;
                    if (ConfigurationManager.AppSettings[KeyErrorFallback] != null)
                        bool.TryParse(ConfigurationManager.AppSettings[KeyErrorFallback], out _fallbackToEPiServerError);
                }
                return _fallbackToEPiServerError;
            }
        }

        /// <summary>
        /// Resource extensions to be ignored.
        /// </summary>
        public List<string> IgnoredResourceExtensions
        {
            get
            {
                if (_ignoredResourceExtensions == null)
                {
                    var ignoredExtensions =
                        string.IsNullOrEmpty(Bvn404HandlerConfiguration.Instance.IgnoredResourceExtensions)
                            ? DefIgnoredExtensions.Split(',')
                            : Bvn404HandlerConfiguration.Instance.IgnoredResourceExtensions.Split(',');
                    _ignoredResourceExtensions = new List<string>(ignoredExtensions);
                }
                return _ignoredResourceExtensions;
            }
        }

        /// <summary>
        /// The mode to use for the 404 handler
        /// </summary>
        public FileNotFoundMode FileNotFoundHandlerMode
        {
            get
            {
                if (_handlerModeIsRead == false)
                {
                    var mode = Bvn404HandlerConfiguration.Instance.HandlerMode ?? DefNotfoundMode.ToString();

                    try
                    {
                        _handlerMode = (FileNotFoundMode)Enum.Parse(typeof(FileNotFoundMode), mode, true /* Ignores case */);
                    }
                    catch
                    {
                        _handlerMode = DefNotfoundMode;
                    }
                    _handlerModeIsRead = true;
                }

                return _handlerMode;
            }
        }

        /// <summary>
        /// The mode to use for the 404 handler
        /// </summary>
        public LoggerMode Logging
        {
            get
            {
                var mode = Bvn404HandlerConfiguration.Instance.Logging ?? DefLogging.ToString();

                try
                {
                    _logging = (LoggerMode)Enum.Parse(typeof(LoggerMode), mode, true /* Ignores case */);
                }
                catch
                {
                    _logging = DefLogging;
                }

                return _logging;
            }
        }

        /// <summary>
        /// Will hostname be included when logging unhandled 404s.
        /// </summary>
        public bool LogWithHostname
        {
            get
            {
                return Bvn404HandlerConfiguration.Instance.LogWithHostname;
            }
        }


        /// <summary>
        /// The virtual path to the 404 handler aspx file.
        /// </summary>
        public string FileNotFoundHandlerPage => string.IsNullOrEmpty(Bvn404HandlerConfiguration.Instance.FileNotFoundPage)
                                                            ? DefNotfoundPage
                                                            : Bvn404HandlerConfiguration.Instance.FileNotFoundPage;

        /// <summary>
        /// The relative path to the custom redirects xml file, including the name of the
        /// xml file. The 404 handler will map the result to a server path.
        /// </summary>
        public string CustomRedirectsXmlFile
        {
            get
            {
                if (Bvn404HandlerConfiguration.Instance != null &&
                    string.IsNullOrEmpty(Bvn404HandlerConfiguration.Instance.RedirectsXmlFile) == false)
                {
                    return Bvn404HandlerConfiguration.Instance.RedirectsXmlFile;
                }

                return DefRedirectsXmlFile;
            }
        }


        /// <summary>
        /// BufferSize for logging of redirects.
        /// </summary>
        public int BufferSize
        {
            get
            {
                if (Bvn404HandlerConfiguration.Instance != null && Bvn404HandlerConfiguration.Instance.BufferSize != -1)
                {
                    return Bvn404HandlerConfiguration.Instance.BufferSize;
                }

                return DefBufferSize;
            }
        }

        /// <summary>
        /// ThreshHold value for redirect logging.
        /// </summary>
        public int ThreshHold
        {
            get
            {
                if (Bvn404HandlerConfiguration.Instance != null && Bvn404HandlerConfiguration.Instance.Threshold != -1)
                {
                    return Bvn404HandlerConfiguration.Instance.Threshold;
                }

                return DefThreshhold;
            }
        }

        public IEnumerable<string> ProviderTypes
        {
            get
            {
                var providers = Bvn404HandlerConfiguration.Instance?.Bvn404HandlerProviders;
                if (providers != null)
                {
                    foreach (Bvn404HandlerProvider provider in providers)
                    {
                        yield return provider.Type;
                    }
                }
            }
        }

        public IEnumerable<INotFoundHandler> Providers => ProviderTypes
            .Select(Type.GetType)
            .Where(NotNull)
            .Select(Provider);

        private static INotFoundHandler Provider(Type t) => (INotFoundHandler)Activator.CreateInstance(t);

        private static bool NotNull(Type t) => t != null;
    }
}
