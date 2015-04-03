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

using IdentityServer3.WsFederation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer3.WsFederation.Services
{
    /// <summary>
    /// In-memory service for relying party configuration
    /// </summary>
    public class InMemoryRelyingPartyService : IRelyingPartyService
    {
        readonly IEnumerable<RelyingParty> _rps;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryRelyingPartyService"/> class.
        /// </summary>
        /// <param name="rps">The RPS.</param>
        public InMemoryRelyingPartyService(IEnumerable<RelyingParty> rps)
        {
            _rps = rps;
        }

        /// <summary>
        /// Retrieves a relying party by realm.
        /// </summary>
        /// <param name="realm">The realm.</param>
        /// <returns>
        /// The relying party
        /// </returns>
        public Task<RelyingParty> GetByRealmAsync(string realm)
        {
            return Task.FromResult(_rps.FirstOrDefault(rp => rp.Realm == realm && rp.Enabled));
        }
    }
}