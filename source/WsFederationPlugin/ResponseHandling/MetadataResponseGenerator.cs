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

using IdentityModel.Constants;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services;
using System;
using System.ComponentModel;
using System.IdentityModel.Metadata;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

#pragma warning disable 1591

namespace IdentityServer3.WsFederation.ResponseHandling
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MetadataResponseGenerator
    {
        private readonly IdentityServerOptions _options;
        private readonly IDictionary<string, object> _environment;

        public MetadataResponseGenerator(IdentityServerOptions options, OwinEnvironmentService owin)
        {
            _options = options;
            _environment = owin.Environment;
        }

        private string IssuerUri
        {
            get
            {
                return _environment.GetIdentityServerIssuerUri();
            }
        }

        public EntityDescriptor Generate(string wsfedEndpoint)
        {
            var applicationDescriptor = GetApplicationDescriptor(wsfedEndpoint);
            var tokenServiceDescriptor = GetTokenServiceDescriptor(wsfedEndpoint);

            var id = new EntityId(IssuerUri);
            var entity = new EntityDescriptor(id);
            entity.SigningCredentials = new X509SigningCredentials(_options.SigningCertificate);
            entity.RoleDescriptors.Add(applicationDescriptor);
            entity.RoleDescriptors.Add(tokenServiceDescriptor);

            return entity;
        }

        private SecurityTokenServiceDescriptor GetTokenServiceDescriptor(string wsfedEndpoint)
        {
            var tokenService = new SecurityTokenServiceDescriptor();
            tokenService.ServiceDescription = _options.SiteName;
            tokenService.Keys.Add(GetSigningKeyDescriptor(_options.SigningCertificate));
            if (_options.SecondarySigningCertificate != null)
            {
                tokenService.Keys.Add(GetSigningKeyDescriptor(_options.SecondarySigningCertificate));
            }

            tokenService.PassiveRequestorEndpoints.Add(new EndpointReference(wsfedEndpoint));
            tokenService.SecurityTokenServiceEndpoints.Add(new EndpointReference(wsfedEndpoint));

            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.OasisWssSaml11TokenProfile11));
            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.OasisWssSaml2TokenProfile11));
            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.JsonWebToken));

            tokenService.ProtocolsSupported.Add(new Uri("http://docs.oasis-open.org/wsfed/federation/200706"));

            return tokenService;
        }

        private ApplicationServiceDescriptor GetApplicationDescriptor(string wsfedEndpoint)
        {
            var tokenService = new ApplicationServiceDescriptor();
            tokenService.ServiceDescription = _options.SiteName;
            tokenService.Keys.Add(GetEncryptionDescriptor(_options.SigningCertificate));
            tokenService.Keys.Add(GetSigningKeyDescriptor(_options.SigningCertificate));
            if (_options.SecondarySigningCertificate != null)
            {
                tokenService.Keys.Add(GetEncryptionDescriptor(_options.SecondarySigningCertificate));
                tokenService.Keys.Add(GetSigningKeyDescriptor(_options.SecondarySigningCertificate));
            }

            tokenService.PassiveRequestorEndpoints.Add(new EndpointReference(wsfedEndpoint));
            tokenService.Endpoints.Add(new EndpointReference(wsfedEndpoint));

            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.OasisWssSaml11TokenProfile11));
            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.OasisWssSaml2TokenProfile11));
            tokenService.TokenTypesOffered.Add(new Uri(TokenTypes.JsonWebToken));

            tokenService.ProtocolsSupported.Add(new Uri("http://docs.oasis-open.org/wsfed/federation/200706"));

            return tokenService;
        }

        private KeyDescriptor GetSigningKeyDescriptor(X509Certificate2 certificate)
        {
            var clause = new X509SecurityToken(certificate).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>();
            var key = new KeyDescriptor(new SecurityKeyIdentifier(clause));
            key.Use = KeyType.Signing;

            return key;
        }

        private KeyDescriptor GetEncryptionDescriptor(X509Certificate2 certificate)
        {
            var clause = new X509SecurityToken(certificate).CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause>();
            var key = new KeyDescriptor(new SecurityKeyIdentifier(clause));
            key.Use = KeyType.Encryption;

            return key;
        }
    }
}