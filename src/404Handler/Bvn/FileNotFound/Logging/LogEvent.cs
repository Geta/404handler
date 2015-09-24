using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVNetwork.Bvn.FileNotFound.Logging
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