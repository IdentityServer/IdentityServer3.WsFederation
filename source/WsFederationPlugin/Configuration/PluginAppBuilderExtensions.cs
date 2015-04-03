/*
 * Copyright 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer3.WsFederation.Configuration;
using IdentityServer3.WsFederation.Hosting;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;

namespace IdentityServer3.Core.Configuration
{
    /// <summary>
    /// Extension methods for IAppBuilder to configure the WS-Federation plugin
    /// </summary>
    public static class PluginAppBuilderExtensions
    {
        /// <summary>
        /// Add the WS-Federation plugin to the IdentityServer pipeline.
        /// </summary>
        /// <param name="app">The appBuilder.</param>
        /// <param name="options">The options.</param>
        /// <returns>The appBuilder</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static IAppBuilder UseWsFederationPlugin(this IAppBuilder app, WsFederationPluginOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            options.Validate();

            options.IdentityServerOptions.ProtocolLogoutUrls.Add(options.LogoutUrl);

            app.Map(options.MapPath, wsfedApp =>
                {
                    wsfedApp.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationType = WsFederationPluginOptions.CookieName,
                        AuthenticationMode = AuthenticationMode.Passive,
                        CookieName = options.IdentityServerOptions.AuthenticationOptions.CookieOptions.Prefix + WsFederationPluginOptions.CookieName,
                    });

                    wsfedApp.Use<AutofacContainerMiddleware>(AutofacConfig.Configure(options));
                    Microsoft.Owin.Infrastructure.SignatureConversions.AddConversions(app);
                    wsfedApp.UseWebApi(WebApiConfig.Configure(options));
                });

            return app;
        }
    }
}