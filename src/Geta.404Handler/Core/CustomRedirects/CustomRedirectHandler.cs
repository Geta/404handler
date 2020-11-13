// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Upgrade;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    /// <summary>
    /// Handler for custom redirects. Loads and caches the list of custom redirects
    /// to ensure performance.
    /// </summary>
    public class CustomRedirectHandler : IRedirectHandler
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private static readonly object CacheLock = new object();
        private const string CacheKeyCustomRedirectHandlerInstance = "BvnCustomRedirectHandler";
        private CustomRedirectCollection _customRedirects;
        private IRedirectsService _redirectsService;
        private IRedirectsService RedirectsService => _redirectsService ??
                                                      (_redirectsService = ServiceLocator.Current.GetInstance<IRedirectsService>());

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

        /// <summary>
        /// Read the custom redirects from the dynamic data store, and
        /// stores them in the CustomRedirect property
        /// </summary>
        protected void LoadCustomRedirects()
        {
            _customRedirects = new CustomRedirectCollection();

            foreach (var redirect in RedirectsService.GetAll())
            {
                try
                {
                    _customRedirects.Add(redirect);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("An error occurred while loading redirect OldUrl = {0}", redirect.OldUrl), ex);
                    CustomRedirectHandlerException = ex.ToString();
                }
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
                lock (CacheLock)
                {
                    // First check if there is a cached version of
                    // this object
                    handler = GetHandlerFromCache();
                    if (handler != null)
                    {
                        Logger.Debug("Returning cached handler, locked.");
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
                        Logger.Error("An error occurred while loading redirects from dds. CustomRedirectHandlerException has now been set.", ex);
                        CustomRedirectHandlerException = ex.ToString();
                        Upgrader.Valid = false;
                    }
                    Logger.Debug("End: Load custom redirects from dynamic data store");

                    // Store in cache
                    StoreHandlerInCache(handler);
                }

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
