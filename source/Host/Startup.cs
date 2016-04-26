using Host.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Host.Config;
using IdentityServer3.WsFederation.Configuration;
using Owin;
using Serilog;

namespace Host
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // setup serilog to use diagnostics trace
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace(outputTemplate: "{Timestamp} [{Level}] ({Name}){NewLine} {Message}{NewLine}{Exception}")
                .CreateLogger();

            app.Map("/core", coreApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryUsers(Users.Get())
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());

                var options = new IdentityServerOptions
                {
                    SiteName = "IdentityServer3 with WS-Federation",

                    SigningCertificate = Certificate.Get(),
                    Factory = factory,
                    PluginConfiguration = ConfigurePlugins,
                };

                coreApp.UseIdentityServer(options);
            });
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var wsFedOptions = new WsFederationPluginOptions(options);
            wsFedOptions.Factory.UseInMemoryRelyingParties(RelyingParties.Get());

            pluginApp.UseWsFederationPlugin(wsFedOptions);
        }
    }
}