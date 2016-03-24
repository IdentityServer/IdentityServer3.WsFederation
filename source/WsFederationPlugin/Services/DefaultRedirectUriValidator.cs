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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.WsFederation.Models;

namespace IdentityServer3.WsFederation.Services
{
    /// <summary>
    /// Default implementation of redirect URI validator. Validates the URIs against
    /// the trusted URIs configured in the plugin options
    /// </summary>
    public class DefaultRedirectUriValidator : IRedirectUriValidator
    {
        /// <summary>
        /// Checks if a given URI string is in a collection of strings (using ordinal ignore case comparison)
        /// </summary>
        /// <param name="uris">The uris.</param>
        /// <param name="requestedUri">The requested URI.</param>
        /// <returns></returns>
        protected bool StringCollectionContainsString(IEnumerable<string> uris, string requestedUri)
        {
            if (uris == null) return false;

            return uris.Contains(requestedUri, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether a post logout URI is valid.
        /// </summary>
        /// <param name="requestedUri">The requested URI.</param>
        /// <param name="relyingParty"></param>
        /// <returns>
        ///   <c>true</c> is the URI is valid; <c>false</c> otherwise.
        /// </returns>
        public virtual Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, RelyingParty relyingParty)
        {
            if (relyingParty == null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(StringCollectionContainsString(relyingParty.PostLogoutRedirectUris, requestedUri));
        }
    }
}