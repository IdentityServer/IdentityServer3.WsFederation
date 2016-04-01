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

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.WsFederation.Validation;

namespace IdentityServer3.WsFederation.Services
{
    /// <summary>
    /// Default custom request validator
    /// </summary>
    public class DefaultCustomWsFederationClaimsService : ICustomWsFederationClaimsService
    {
        /// <summary>
        /// Transforms claims before they are sent back to relying party in response to sign in.
        /// </summary>
        /// <param name="validationResult">The validated request.</param>
        /// <param name="mappedClaims">Suggested claims</param>
        /// <returns>Final claims to include in response</returns>
        public Task<IEnumerable<Claim>> TransformClaimsAsync(SignInValidationResult validationResult, IEnumerable<Claim> mappedClaims)
        {
            return Task.FromResult(mappedClaims);
        }
    }
}