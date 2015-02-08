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
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityModel.Constants;

namespace Thinktecture.IdentityServer.WsFederation.Models
{
    /// <summary>
    /// Models a WS-Federation relying party
    /// </summary>
    public class RelyingParty
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RelyingParty"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the realm.
        /// </summary>
        /// <value>
        /// The realm.
        /// </value>
        public string Realm { get; set; }

        /// <summary>
        /// Gets or sets the reply URL.
        /// </summary>
        /// <value>
        /// The reply URL.
        /// </value>
        public string ReplyUrl { get; set; }

        /// <summary>
        /// Gets or sets the type of the token.
        /// </summary>
        /// <value>
        /// The type of the token.
        /// </value>
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the token life time in minutes.
        /// </summary>
        /// <value>
        /// The token life time.
        /// </value>
        public int TokenLifeTime { get; set; }

        /// <summary>
        /// Gets or sets the encrypting certificate.
        /// </summary>
        /// <value>
        /// The encrypting certificate.
        /// </value>
        public X509Certificate2 EncryptingCertificate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include all claims for the user in the token.
        /// </summary>
        /// <value>
        /// <c>true</c> if include all claims for user; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeAllClaimsForUser { get; set; }

        /// <summary>
        /// Gets or sets the default claim type mapping prefix.
        /// </summary>
        /// <value>
        /// The default claim type mapping prefix.
        /// </value>
        public string DefaultClaimTypeMappingPrefix { get; set; }

        /// <summary>
        /// Gets or sets the name identifier format (SAML only).
        /// </summary>
        /// <value>
        /// The SAML name identifier format.
        /// </value>
        public string SamlNameIdentifierFormat { get; set; }

        /// <summary>
        /// Gets or sets the claim mappings.
        /// </summary>
        /// <value>
        /// The claim mappings.
        /// </value>
        public Dictionary<string, string> ClaimMappings { get; set; }

        /// <summary>
        /// Gets or sets the tokenn signature algorithm.
        /// </summary>
        /// <value>
        /// The signature algorithm.
        /// </value>
        public string SignatureAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the digest algorithm.
        /// </summary>
        /// <value>
        /// The digest algorithm.
        /// </value>
        public string DigestAlgorithm { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelyingParty"/> class.
        /// </summary>
        public RelyingParty()
        {
            ClaimMappings = new Dictionary<string, string>();
            TokenType = TokenTypes.Saml2TokenProfile11;
            
            SignatureAlgorithm = SecurityAlgorithms.RsaSha256Signature;
            DigestAlgorithm = SecurityAlgorithms.Sha256Digest;

            TokenLifeTime = 600;
            Enabled = true;
        }
    }
}