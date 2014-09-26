/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using System;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationPluginOptions
    {
        public const string CookieName = "IdSvr.WsFedTracking";

        public string LogoutUrl
        {
            get
            {
                return MapPath + "/signout";
            }
        }
        
        public IdentityServerOptions IdentityServerOptions { get; set; }
        public WsFederationServiceFactory Factory { get; set; }
        public EndpointSettings MetadataEndpoint { get; set; }
        
        public IDataProtector DataProtector
        {
            get
            {
                return IdentityServerOptions.DataProtector;
            }
        }

        public string MapPath { get; set; }

        public WsFederationPluginOptions()
        {
            MapPath = "/wsfed";
            MetadataEndpoint = EndpointSettings.Enabled;
        }

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