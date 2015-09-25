using System;
using System.Collections.Generic;
using System.Linq;
using BVNetwork.NotFound.Core.Data;
using log4net;

namespace BVNetwork.NotFound.Core.Logging
{
    public static class Logger
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LogRequest(string oldUrl, string referer)
        {
            int bufferSize = Configuration.Configuration.BufferSize;
      
            if (LogEvents == null)
                LogEvents = new List<LogEvent>();
            LogEvents.Add(new LogEvent(oldUrl, DateTime.Now, referer));
            if (LogEvents.Count >= bufferSize)
            {
                var logEvents2 = LogEvents;
                LogRequests(logEvents2);
                LogEvents = new List<LogEvent>();
            }
            
        }

        private static void LogRequests(List<LogEvent> logEvents)
        {
            _log.Info("Logging 404 errors to database");
            int bufferSize = Configuration.Configuration.BufferSize;
            int threshold = Configuration.Configuration.ThreshHold;
            var start = logEvents.First().Requested;
            var end = logEvents.Last().Requested;
            var diff = (end - start).Seconds;

            if ((diff != 0 && bufferSize / diff <= threshold) || bufferSize == 0)
            {
                var dba = DataAccessBaseEx.GetWorker();
                foreach (LogEvent logEvent in logEvents)
                {
                    dba.LogRequestToDb(logEvent.OldUrl, logEvent.Referer, logEvent.Requested);
                }
                _log.Info(string.Format("{0} 404 request(s) has been stored to the database.", bufferSize));
            }
            else
                _log.Warn("404 requests have been made too frequents (exceeded the threshold). Requests not logged to database.");
           
        }

        public static List<LogEvent> LogEvents { get; set; }




    }
}