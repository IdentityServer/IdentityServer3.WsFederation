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

using IdentityServer3.Core.Models;
using IdentityServer3.WsFederation.Models;
using System.ComponentModel;
using System.IdentityModel.Services;
using System.Security.Claims;

#pragma warning disable 1591

namespace IdentityServer3.WsFederation.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SignInValidationResult
    {
        public bool IsError { get; set; }
        public string Error { get; set; }
        public string ErrorMessage { get; set; }

        public bool IsSignInRequired { get; set; }
        public SignInMessage SignInMessage { get; set; }

        public RelyingParty RelyingParty { get; set; }
        public SignInRequestMessage SignInRequestMessage { get; set; }
        
        public string ReplyUrl { get; set; }
        public string HomeRealm { get; set; }
        public string Federation { get; set; }

        public ClaimsPrincipal Subject { get; set; }
    }
}