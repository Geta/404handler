using System.Collections.Generic;

namespace BVNetwork.NotFound.Core.Configuration
{
    public interface IConfiguration
    {
        int BufferSize { get; }
        string CustomRedirectsXmlFile { get; }
        bool FallbackToEPiServerErrorExceptionManager { get; }
        FileNotFoundMode FileNotFoundHandlerMode { get; }
        string FileNotFoundHandlerPage { get; }
        List<string> IgnoredResourceExtensions { get; }
        LoggerMode Logging { get; }
        int ThreshHold { get; }
        IEnumerable<INotFoundHandler> Providers { get; }
        bool LogWithHostname { get; }
    }
}