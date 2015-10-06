using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BVNetwork.NotFound.Core.Data;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Logging
{
    public class RequestLogger
    {
        private static readonly RequestLogger instance = new RequestLogger();

        public static RequestLogger Instance
        {
            get
            {
                return InternalInstance;
            }
        }

        internal static RequestLogger InternalInstance
        {
            get
            {
                return instance;
            }
        }

        private RequestLogger()
        {

            LogQueue = new List<LogEvent>();
        }
        private static readonly ILogger Logger = LogManager.GetLogger();

        public void LogRequest(string oldUrl, string referrer)
        {
            int bufferSize = Configuration.Configuration.BufferSize;     
            if (LogQueue.Count >= bufferSize)
            {
                lock (LogQueue)
                {
                    try
                    {
                        if (LogQueue.Count >= bufferSize)
                            LogRequests(LogQueue);
                        LogQueue = new List<LogEvent>();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("An error occured whil trying to log 404 errors. ", ex);
                        LogQueue = new List<LogEvent>();
                    }
                }
            }
            LogQueue.Add(new LogEvent(oldUrl, DateTime.Now, referrer));
        }

        private void LogRequests(List<LogEvent> logEvents)
        {
            Logger.Debug("Logging 404 errors to database");
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
                Logger.Debug(string.Format("{0} 404 request(s) has been stored to the database.", bufferSize));
            }
            else
                Logger.Warning("404 requests have been made too frequents (exceeded the threshold). Requests not logged to database.");

        }
        private List<LogEvent> LogQueue { get; set; }

    }

}