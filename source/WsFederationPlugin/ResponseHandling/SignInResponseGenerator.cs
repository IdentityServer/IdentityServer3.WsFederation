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

using IdentityModel.Tokens;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.WsFederation.Logging;
using IdentityServer3.WsFederation.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer3.WsFederation.ResponseHandling
{
    [EditorBrowsable(EditorBrowsableState.Never)]
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
            var profileClaims = new List<Claim>();
            var mappedClaims = new List<Claim>();

            // get all claims from user service
            if (validationResult.RelyingParty.IncludeAllClaimsForUser)
            {
                var ctx = new ProfileDataRequestContext
                {
                    Subject = validationResult.Subject
                };
                await _users.GetProfileDataAsync(ctx);
                
                profileClaims = ctx.IssuedClaims.ToList();
            }
            else
            {
                // get only claims that are explicitly mapped (if any)
                var claimTypes = validationResult.RelyingParty.ClaimMappings.Keys;

                if (claimTypes.Any())
                {
                    var ctx = new ProfileDataRequestContext
                    {
                        Subject = validationResult.Subject,
                        RequestedClaimTypes = claimTypes
                    };
                    await _users.GetProfileDataAsync(ctx);

                    profileClaims = ctx.IssuedClaims.ToList();
                }
            }
            
            foreach (var claim in profileClaims)
            {
                string mappedType;

                // if an explicit mapping exists, use it
                if (validationResult.RelyingParty.ClaimMappings.TryGetValue(claim.Type, out mappedType))
                {
                    // if output claim is a SAML name ID - check is any name ID format is configured
                    if (mappedType == ClaimTypes.NameIdentifier)
                    {
                        var nameId = new Claim(ClaimTypes.NameIdentifier, claim.Value);
                        if (!string.IsNullOrEmpty(validationResult.RelyingParty.SamlNameIdentifierFormat))
                        {
                            nameId.Properties[ClaimProperties.SamlNameIdentifierFormat] = validationResult.RelyingParty.SamlNameIdentifierFormat;
                        }

                        mappedClaims.Add(nameId);
                    }
                    else
                    {
                        mappedClaims.Add(new Claim(mappedType, claim.Value));
                    }
                }
                else
                {
                    // otherwise pass-through the claims if flag is set
                    if (validationResult.RelyingParty.IncludeAllClaimsForUser)
                    {
                        string newType = claim.Type;

                        // if prefix is configured, prefix the claim type
                        if (!string.IsNullOrWhiteSpace(validationResult.RelyingParty.DefaultClaimTypeMappingPrefix))
                        {
                            newType = validationResult.RelyingParty.DefaultClaimTypeMappingPrefix + newType;
                        }

                        mappedClaims.Add(new Claim(newType, claim.Value));
                    }
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
                SigningCredentials = new X509SigningCredentials(_options.SigningCertificate, validationResult.RelyingParty.SignatureAlgorithm, validationResult.RelyingParty.DigestAlgorithm),
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