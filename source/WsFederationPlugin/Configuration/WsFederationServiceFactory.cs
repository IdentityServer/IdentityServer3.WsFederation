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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.WsFederation.Logging;
using IdentityServer3.WsFederation.Services;
using System;
using System.Collections.Generic;

namespace IdentityServer3.WsFederation.Configuration
{
    /// <summary>
    /// The WS-Federation plugin service factory
    /// </summary>
    public class WsFederationServiceFactory
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        // keep list of any additional dependencies the 
        // hosting application might need. these will be
        // added to the DI container
        readonly List<Registration> _registrations = new List<Registration>();

        /// <summary>
        /// Gets the a list of additional dependencies.
        /// </summary>
        /// <value>
        /// The dependencies.
        /// </value>
        public IEnumerable<Registration> Registrations
        {
            get { return _registrations; }
        }

        /// <summary>
        /// Adds a registration to the dependency list
        /// </summary>
        /// <typeparam name="T">Type of the dependency</typeparam>
        /// <param name="registration">The registration.</param>
        public void Register<T>(Registration<T> registration)
            where T : class
        {
            _registrations.Add(registration);
        }

        ///////////////////////
        // mandatory (external)
        ///////////////////////

        // mandatory (external)
        /// <summary>
        /// Gets or sets the user service.
        /// </summary>
        /// <value>
        /// The user service.
        /// </value>
        public Registration<IUserService> UserService { get; set; }

        /// <summary>
        /// Gets or sets the relying party service.
        /// </summary>
        /// <value>
        /// The relying party service.
        /// </value>
        public Registration<IRelyingPartyService> RelyingPartyService { get; set; }

        // optional
        /// <summary>
        /// Gets or sets the custom request validator service.
        /// </summary>
        /// <value>
        /// The custom request validator service.
        /// </value>
        public Registration<ICustomWsFederationRequestValidator> CustomRequestValidator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFederationServiceFactory"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public WsFederationServiceFactory(IdentityServerServiceFactory factory)
        {
            UserService = factory.UserService;
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public void Validate()
        {
            if (UserService == null) LogAndStop("UserService not configured");
            if (RelyingPartyService == null) LogAndStop("RelyingPartyService not configured");
        }

        private void LogAndStop(string message)
        {
            Logger.Error(message);
            throw new InvalidOperationException(message);
        }
    }
}