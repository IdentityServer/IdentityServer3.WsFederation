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

using IdentityModel;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer3.WsFederation.Events
{
    public static class WsFederationEventExtensions
    {
        public static async Task RaiseSuccessfulWsFederationEndpointEventAsync(this IEventService events, string operation, string realm, ClaimsPrincipal subject, string url)
        {
            var evt = new Event<WsFederationEndpointDetail>(
                EventConstants.Categories.Endpoints,
                "WS-Federation endpoint success",
                EventTypes.Success,
                WsFederationEventConstants.Ids.WsFederationEndpointSuccess,
                new WsFederationEndpointDetail
                {
                    Operation = operation,
                    Realm = realm,
                    Subject = GetSubjectId(subject),
                    Url = url
                });

            await events.RaiseAsync(evt);
        }

        public static async Task RaiseFailureWsFederationEndpointEventAsync(this IEventService events, string operation, string realm, ClaimsPrincipal subject, string url, string error)
        {
            var evt = new Event<WsFederationEndpointDetail>(
                 EventConstants.Categories.Endpoints,
                 "WS-Federation endpoint failure",
                 EventTypes.Failure,
                 WsFederationEventConstants.Ids.WsFederationEndpointFailure,
                 new WsFederationEndpointDetail
                 {
                     Operation = operation,
                     Realm = realm,
                     Subject = GetSubjectId(subject),
                     Url = url
                 }, error);

            await events.RaiseAsync(evt);
        }

        private static string GetSubjectId(ClaimsPrincipal principal)
        {
            if (principal == null) return "anonymous";

            var subClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
            if (subClaim != null)
            {
                return subClaim.Value;
            }

            return "anonymous";
        }
    }
}