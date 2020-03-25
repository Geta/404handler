// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace BVNetwork.NotFound.Core.Logging
{
    public class LogEvent
    {

        public LogEvent(string oldUrl, DateTime requested, string referer)
        {
            OldUrl = oldUrl;
            Requested = requested;
            Referer = referer;
        }

        public string OldUrl { get; set; }
        public DateTime Requested { get; set; }
        public string Referer { get; set; }
    }
}