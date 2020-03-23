// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace BVNetwork.NotFound.Core.Initialization
{
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class DependencyInjectionInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IConfiguration>(Configuration.Configuration.Instance);
            context.Services.AddSingleton<IRequestLogger>(RequestLogger.Instance);
            context.Services.AddTransient<IRedirectHandler>(_ => CustomRedirectHandler.Current); // Load per-request as it is read from the cache

            context.Services.AddTransient<IRedirectsService, DefaultRedirectsService>();
            context.Services.AddTransient<IRepository<CustomRedirect>, SqlRedirectRepository>();
            context.Services.AddTransient<IRedirectLoader, SqlRedirectRepository>();
        }

        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}