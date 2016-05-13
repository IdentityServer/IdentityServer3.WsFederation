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
using System.ComponentModel;
using System.IdentityModel.Services;
using System.Threading.Tasks;
using IdentityServer3.WsFederation.Logging;
using IdentityServer3.WsFederation.Logging.Models;
using IdentityServer3.WsFederation.Services;

#pragma warning disable 1591

namespace IdentityServer3.WsFederation.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SignOutValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IRelyingPartyService _relyingParties;

        public SignOutValidator(IRelyingPartyService relyingParties)
        {
            _relyingParties = relyingParties;
        }

        public async Task<SignOutValidationResult> ValidateAsync(SignOutRequestMessage message)
        {
            Logger.Info("Start WS-Federation signout request validation");
            var result = new SignOutValidationResult();

            // check realm
            var realm = message.GetParameter("wtrealm");
            if (String.IsNullOrWhiteSpace(realm))
            {
                LogError("Realm has not been provided", result);
            }
            result.Realm = realm;
            var rp = await _relyingParties.GetByRealmAsync(realm);
            if (rp == null || rp.Enabled == false)
            {
                LogError("Relying party not found: " + realm, result);

                return new SignOutValidationResult
                {
                    IsError = true,
                    Error = "invalid_relying_party"
                };
            }

            result.RelyingParty = rp;

            LogSuccess(result);
            return result;
        }

        private void LogSuccess(SignOutValidationResult result)
        {
            var log = LogSerializer.Serialize(new SignOutValidationLog(result));
            Logger.InfoFormat("End WS-Federation signout request validation\n{0}", log);
        }

        private void LogError(string message, SignOutValidationResult result)
        {
            var log = LogSerializer.Serialize(new SignOutValidationLog(result));
            Logger.ErrorFormat("{0}\n{1}", message, log);
        }
    }
}
