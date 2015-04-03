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

using IdentityServer3.Core.Extensions;
using IdentityServer3.WsFederation.Validation;

namespace IdentityServer3.WsFederation.Logging
{
    internal class SignInValidationLog
    {
        public SignInValidationLog(SignInValidationResult result)
        {
            if (result.RelyingParty != null)
            {
                Realm = result.RelyingParty.Realm;
                RelyingPartyName = result.RelyingParty.Name;
            }

            if (Subject != null)
            {
                Subject = result.Subject.GetSubjectId();
            }

            ReplyUrl = result.ReplyUrl;
            HomeRealm = result.HomeRealm;
            Federation = result.Federation;
        }

        public string Realm { get; set; }
        public string RelyingPartyName { get; set; }
        public string ReplyUrl { get; set; }
        public string HomeRealm { get; set; }
        public string Federation { get; set; }
        public string Subject { get; set; }
    }
}