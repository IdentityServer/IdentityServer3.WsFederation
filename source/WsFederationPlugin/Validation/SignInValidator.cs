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
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.WsFederation.Logging;
using Thinktecture.IdentityServer.WsFederation.Services;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.WsFederation.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SignInValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IRelyingPartyService _relyingParties;

        public SignInValidator(IRelyingPartyService relyingParties)
        {
            _relyingParties = relyingParties;
        }

        public async Task<SignInValidationResult> ValidateAsync(SignInRequestMessage message, ClaimsPrincipal subject)
        {
            Logger.Info("Start WS-Federation signin request validation");
            var result = new SignInValidationResult();

            // parse whr
            if (!String.IsNullOrWhiteSpace(message.HomeRealm))
            {
                result.HomeRealm = message.HomeRealm;
            }

            // parse wfed
            if (!String.IsNullOrWhiteSpace(message.Federation))
            {
                result.Federation = message.Federation;
            }

            if (!subject.Identity.IsAuthenticated)
            {
                result.IsSignInRequired = true;
                return result;
            }

            // check realm
            var rp = await _relyingParties.GetByRealmAsync(message.Realm);

            if (rp == null || rp.Enabled == false)
            {
                LogError("Relying party not found: " + message.Realm, result);

                return new SignInValidationResult
                {
                    IsError = true,
                    Error = "invalid_relying_party"
                };
            }

            result.ReplyUrl = rp.ReplyUrl;
            result.RelyingParty = rp;
            result.SignInRequestMessage = message;
            result.Subject = subject;

            LogSuccess(result);
            return result;
        }

        private void LogSuccess(SignInValidationResult result)
        {
            var log = LogSerializer.Serialize(new SignInValidationLog(result));
            Logger.InfoFormat("End WS-Federation signin request validation\n{0}", log);
        }

        private void LogError(string message, SignInValidationResult result)
        {
            var log = LogSerializer.Serialize(new SignInValidationLog(result));
            Logger.ErrorFormat("{0}\n{1}", message, log);
        }
    }
}