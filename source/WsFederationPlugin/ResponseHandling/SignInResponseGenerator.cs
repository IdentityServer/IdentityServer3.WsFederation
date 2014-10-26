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
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Validation;

namespace Thinktecture.IdentityServer.WsFederation.ResponseHandling
{
    public class SignInResponseGenerator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IdentityServerOptions _options;
        private readonly IUserService _users;
        
        public SignInResponseGenerator(IdentityServerOptions options, IUserService users)
        {
            _options = options;
            _users = users;
        }

        public async Task<SignInResponseMessage> GenerateResponseAsync(SignInValidationResult validationResult)
        {
            Logger.Info("Creating WS-Federation signin response");

            // create subject
            var outgoingSubject = await CreateSubjectAsync(validationResult);

            // create token for user
            var token = CreateSecurityToken(validationResult, outgoingSubject);

            // return response
            var rstr = new RequestSecurityTokenResponse
            {
                AppliesTo = new EndpointReference(validationResult.RelyingParty.Realm),
                Context = validationResult.SignInRequestMessage.Context,
                ReplyTo = validationResult.ReplyUrl,
                RequestedSecurityToken = new RequestedSecurityToken(token)
            };

            var serializer = new WSFederationSerializer(
                new WSTrust13RequestSerializer(),
                new WSTrust13ResponseSerializer());

            var mgr = SecurityTokenHandlerCollectionManager.CreateEmptySecurityTokenHandlerCollectionManager();
            mgr[SecurityTokenHandlerCollectionManager.Usage.Default] = CreateSupportedSecurityTokenHandler();
            
            var responseMessage = new SignInResponseMessage(
                new Uri(validationResult.ReplyUrl),
                rstr,
                serializer,
                new WSTrustSerializationContext(mgr));

            return responseMessage;
        }

        private async Task<ClaimsIdentity> CreateSubjectAsync(SignInValidationResult validationResult)
        {
            var claims = await _users.GetProfileDataAsync(
                validationResult.Subject, 
                validationResult.RelyingParty.ClaimMappings.Keys);

            var mappedClaims = new List<Claim>();

            foreach (var claim in claims)
            {
                string mappedType;
                if (validationResult.RelyingParty.ClaimMappings.TryGetValue(claim.Type, out mappedType))
                {
                    mappedClaims.Add(new Claim(mappedType, claim.Value));
                }
            }

            if (validationResult.Subject.GetAuthenticationMethod() == Constants.AuthenticationMethods.Password)
            {
                mappedClaims.Add(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Password));
                mappedClaims.Add(AuthenticationInstantClaim.Now);
            }
            
            return new ClaimsIdentity(mappedClaims, "idsrv");
        }

        private SecurityToken CreateSecurityToken(SignInValidationResult validationResult, ClaimsIdentity outgoingSubject)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                AppliesToAddress = validationResult.RelyingParty.Realm,
                Lifetime = new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(validationResult.RelyingParty.TokenLifeTime)),
                ReplyToAddress = validationResult.ReplyUrl,
                SigningCredentials = new X509SigningCredentials(_options.SigningCertificate),
                Subject = outgoingSubject,
                TokenIssuerName = _options.IssuerUri,
                TokenType = validationResult.RelyingParty.TokenType
            };

            if (validationResult.RelyingParty.EncryptingCertificate != null)
            {
                descriptor.EncryptingCredentials = new X509EncryptingCredentials(validationResult.RelyingParty.EncryptingCertificate);
            }

            return CreateSupportedSecurityTokenHandler().CreateToken(descriptor);
        }

        private SecurityTokenHandlerCollection CreateSupportedSecurityTokenHandler()
        {
            return new SecurityTokenHandlerCollection(new SecurityTokenHandler[]
            {
                new SamlSecurityTokenHandler(),
                new EncryptedSecurityTokenHandler(),
                new Saml2SecurityTokenHandler(),
                new JwtSecurityTokenHandler()
            });
        }
    }
}