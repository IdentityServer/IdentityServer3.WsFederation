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

using IdentityServer3.Core.Configuration;
using System;

namespace IdentityServer3.WsFederation.Configuration
{
    /// <summary>
    /// WS-Federation plugin options
    /// </summary>
    public class WsFederationPluginOptions
    {
        /// <summary>
        /// The tracking cookie name
        /// </summary>
        public const string CookieName = "IdSvr.WsFedTracking";

        /// <summary>
        /// Gets the logout URL.
        /// </summary>
        /// <value>
        /// The logout URL.
        /// </value>
        public string LogoutUrl
        {
            get
            {
                return MapPath + "/signout";
            }
        }

        /// <summary>
        /// Gets or sets the identity server options.
        /// </summary>
        /// <value>
        /// The identity server options.
        /// </value>
        public IdentityServerOptions IdentityServerOptions { get; set; }

        /// <summary>
        /// Gets or sets the WS-Federation service factory.
        /// </summary>
        /// <value>
        /// The factory.
        /// </value>
        public WsFederationServiceFactory Factory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the metadata endpoint is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if the metadata endpoint is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableMetadataEndpoint { get; set; }

        /// <summary>
        /// Gets the data protector.
        /// </summary>
        /// <value>
        /// The data protector.
        /// </value>
        public IDataProtector DataProtector
        {
            get
            {
                return IdentityServerOptions.DataProtector;
            }
        }

        /// <summary>
        /// Gets or sets the map path.
        /// </summary>
        /// <value>
        /// The map path.
        /// </value>
        public string MapPath { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFederationPluginOptions"/> class.
        /// </summary>
        public WsFederationPluginOptions()
        {
            MapPath = "/wsfed";
            EnableMetadataEndpoint = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFederationPluginOptions"/> class.
        /// Assigns the IdentityServerOptions and the Factory from the IdentityServerOptions.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public WsFederationPluginOptions(IdentityServerOptions options)
            : this()
        {
            if (options == null) throw new ArgumentNullException("options");
            
            this.IdentityServerOptions = options;
            this.Factory = new WsFederationServiceFactory(options.Factory);
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Factory not configured
        /// or
        /// DataProtector not configured
        /// or
        /// Options not configured
        /// </exception>
        public void Validate()
        {
            if (Factory == null)
            {
                throw new ArgumentNullException("Factory not configured");
            }
            if (DataProtector == null)
            {
                throw new ArgumentNullException("DataProtector not configured");
            }
            if (IdentityServerOptions == null)
            {
                throw new ArgumentNullException("Options not configured");
            }
        }
    }
}