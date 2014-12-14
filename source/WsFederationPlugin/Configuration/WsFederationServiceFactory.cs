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
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.WsFederation.Services;

namespace Thinktecture.IdentityServer.WsFederation.Configuration
{
    public class WsFederationServiceFactory
    {
        private static ILog Logger = LogProvider.GetCurrentClassLogger();

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
        public Registration<IUserService> UserService { get; set; }
        public Registration<IRelyingPartyService> RelyingPartyService { get; set; }

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