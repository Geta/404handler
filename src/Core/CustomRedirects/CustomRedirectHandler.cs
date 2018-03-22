using System;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Upgrade;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    /// <summary>
    /// Handler for custom redirects. Loads and caches the list of custom redirects
    /// to ensure performance.
    /// </summary>
    public class CustomRedirectHandler : IRedirectHandler
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private const string CacheKeyCustomRedirectHandlerInstance = "BvnCustomRedirectHandler";
        private CustomRedirectCollection _customRedirects;

        // Should only be instanciated by the static Current method
        protected CustomRedirectHandler()
        {
        }

        /// <summary>
        /// The collection of custom redirects
        /// </summary>
        public CustomRedirectCollection CustomRedirects => _customRedirects;

        public CustomRedirect Find(Uri urlNotFound)
        {
            return CustomRedirects.Find(urlNotFound);
        }

        public CustomRedirect Find(Uri urlNotFound, Uri referrer)
        {
            return CustomRedirects.Find(urlNotFound);
        }

        /// <summary>
        /// Save a collection of redirects, and call method to raise an event in order to clear cache on all servers.
        /// </summary>
        /// <param name="redirects"></param>
        public void SaveCustomRedirects(CustomRedirectCollection redirects)
        {
            var dynamicHandler = new DataStoreHandler();
            foreach (CustomRedirect redirect in redirects)
            {
                // Add redirect
                dynamicHandler.SaveCustomRedirect(redirect);
            }
            DataStoreEventHandlerHook.DataStoreUpdated();
        }

        /// <summary>
        /// Read the custom redirects from the dynamic data store, and
        /// stores them in the CustomRedirect property
        /// </summary>
        protected void LoadCustomRedirects()
        {
            var dynamicHandler = new DataStoreHandler();
            _customRedirects = new CustomRedirectCollection();

            foreach (var redirect in dynamicHandler.GetCustomRedirects(false))
            {
                _customRedirects.Add(redirect);
            }
        }

        /// <summary>
        /// Static method for getting the current custom redirects handler. Will store
        /// the handler in cache after it has been used.
        /// </summary>
        /// <returns>An instanciated CustomRedirectHandler</returns>
        public static CustomRedirectHandler Current
        {
            get
            {
                Logger.Debug("Begin: Get Current CustomRedirectHandler");
                // First check if there is a cached version of
                // this object
                var handler = GetHandlerFromCache();
                if (handler != null)
                {
                    Logger.Debug("Returning cached handler.");
                    Logger.Debug("End: Get Current CustomRedirectHandler");
                    // Got the cached version, return it
                    return handler;
                }

                // Not cached, we need to create it
                handler = new CustomRedirectHandler();
                // Load redirects with standard settings
                Logger.Debug("Begin: Load custom redirects from dynamic data store");
                try
                {
                    handler.LoadCustomRedirects();
                }
                catch (Exception ex)
                {
                    Logger.Error("An error occured while loading redirects from dds. CustomRedirectHandlerException has now been set. Exception:" + ex);
                    CustomRedirectHandlerException = ex.ToString();
                    Upgrader.Valid = false;
                }
                Logger.Debug("End: Load custom redirects from dynamic data store");

                // Store in cache
                StoreHandlerInCache(handler);

                Logger.Debug("End: Get Current CustomRedirectHandler");
                return handler;
            }
        }

        /// <summary>
        /// Clears the redirect cache.
        /// </summary>
        public static void ClearCache()
        {
            EPiServer.CacheManager.Remove(CacheKeyCustomRedirectHandlerInstance);
        }

        /// <summary>
        /// Reload handler in case if it has error and handler was not created properly.
        /// </summary>
        public static void ReloadCustomRedirectHandler()
        {
            CustomRedirectHandlerException = string.Empty;
            var temp = Current;
        }

        /// <summary>
        /// Gets the handler from the cache, if it has been stored there.
        /// </summary>
        /// <returns>An instanciated CustomRedirectHandler if found in the cache, null if not found</returns>
        private static CustomRedirectHandler GetHandlerFromCache()
        {
            return EPiServer.CacheManager.Get(CacheKeyCustomRedirectHandlerInstance) as CustomRedirectHandler;
        }

        /// <summary>
        /// Stores the redirect handler in the cache
        /// </summary>
        private static void StoreHandlerInCache(CustomRedirectHandler handler)
        {
            EPiServer.CacheManager.Insert(CacheKeyCustomRedirectHandlerInstance, handler);
        }

        public static string CustomRedirectHandlerException { get; set; }
    }
}
