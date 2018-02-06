using System;
using System.Collections.Concurrent;
using System.Linq;
using BVNetwork.NotFound.Core.Data;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.Logging
{
    public class RequestLogger : IRequestLogger
    {
        public static RequestLogger Instance => InternalInstance;

        internal static RequestLogger InternalInstance { get; } = new RequestLogger();

        private RequestLogger() { }

        private static readonly ILogger Logger = LogManager.GetLogger();

        public void LogRequest(string oldUrl, string referrer)
        {
            var bufferSize = Configuration.Configuration.Instance.BufferSize;
            if (LogQueue.Count > 0 && LogQueue.Count >= bufferSize)
            {
                lock (LogQueue)
                {
                    try
                    {
                        if (LogQueue.Count >= bufferSize)
                        {
                            LogRequests(LogQueue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("An error occured while trying to log 404 errors. ", ex);
                    }
                }
            }
            LogQueue.Enqueue(new LogEvent(oldUrl, DateTime.UtcNow, referrer));
        }

        private void LogRequests(ConcurrentQueue<LogEvent> logEvents)
        {
            Logger.Debug("Logging 404 errors to database");
            var bufferSize = Configuration.Configuration.Instance.BufferSize;
            var threshold = Configuration.Configuration.Instance.ThreshHold;
            var start = logEvents.First().Requested;
            var end = logEvents.Last().Requested;
            var diff = (end - start).Seconds;

            if ((diff != 0 && bufferSize / diff <= threshold)
                || bufferSize == 0)
            {
                var dba = DataAccessBaseEx.GetWorker();
                while (logEvents.Count > 0)
                {
                    if (logEvents.TryDequeue(out var logEvent))
                    {
                        dba.LogRequestToDb(logEvent.OldUrl, logEvent.Referer, logEvent.Requested);
                    }
                }
                Logger.Debug($"{bufferSize} 404 request(s) has been stored to the database.");
            }
            else
            {
                Logger.Warning("404 requests have been made too frequents (exceeded the threshold). Requests not logged to database.");
            }
        }

        private static ConcurrentQueue<LogEvent> LogQueue { get; } = new ConcurrentQueue<LogEvent>();
    }

}