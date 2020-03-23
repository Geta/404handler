// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

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