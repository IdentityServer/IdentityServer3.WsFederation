/* 
 * Copyright 2014, 2015 Dominick Baier, Brock Allen 
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

using System.Collections.Generic;
using IdentityServer3.Core.Configuration;
using IdentityServer3.WsFederation.Models;
using IdentityServer3.WsFederation.Services;

namespace IdentityServer3.WsFederation.Configuration
{
    /// <summary> 
    /// Extension methods for <see cref="IdentityServer3.WsFederation.Configuration.WsFederationServiceFactory"/> 
    /// </summary> 
    public static class WsFederationServiceFactoryExtensions
    {

        /// <summary> 
        /// Configures the factory to use in-memory relying parties. 
        /// </summary> 
        /// <param name="factory">The factory.</param> 
        /// <param name="relyingParties">The relying parties.</param> 
        /// <returns></returns> 
        public static WsFederationServiceFactory UseInMemoryRelyingParties(this WsFederationServiceFactory factory, IEnumerable<RelyingParty> relyingParties)
        {
            factory.Register(new Registration<IEnumerable<RelyingParty>>(relyingParties));
            factory.RelyingPartyService = new Registration<IRelyingPartyService, InMemoryRelyingPartyService>();

            return factory;
        }
    }
}