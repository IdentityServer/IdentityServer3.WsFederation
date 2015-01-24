using Host.Config;
using Owin;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services.InMemory;
using Thinktecture.IdentityServer.Host.Config;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using Thinktecture.IdentityServer.WsFederation.Models;
using Thinktecture.IdentityServer.WsFederation.Services;

namespace Host
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/core", coreApp =>
            {
                coreApp.Use(async (ctx, next) =>
                {
                    await next();
                });
                
                var factory = InMemoryFactory.Create(
                    users:   Users.Get(),
                    clients: Clients.Get(),
                    scopes:  Scopes.Get());

                var options = new IdentityServerOptions
                {
                    IssuerUri = "https://idsrv3.com",
                    SiteName = "Thinktecture IdentityServer3",

                    SigningCertificate = Certificate.Get(),
                    Factory = factory,
                    PluginConfiguration = ConfigurePlugins,
                };

                coreApp.UseIdentityServer(options);
            });
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var factory = new WsFederationServiceFactory
            {
                UserService = options.Factory.UserService,
                RelyingPartyService = new Registration<IRelyingPartyService>(typeof(InMemoryRelyingPartyService))
            };

            // data sources for in-memory services
            factory.Register(new Registration<IEnumerable<RelyingParty>>(RelyingParties.Get()));

            var wsFedOptions = new WsFederationPluginOptions
            {
                IdentityServerOptions = options,
                Factory = factory
            };

            pluginApp.UseWsFederationPlugin(wsFedOptions);
        }
    }
}