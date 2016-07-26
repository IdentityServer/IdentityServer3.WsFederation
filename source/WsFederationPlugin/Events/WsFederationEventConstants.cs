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
 
 namespace IdentityServer3.WsFederation.Events
{
    public static class WsFederationEventConstants
    {
        public static class Operations
        {
            public const string SignIn = "signin";
            public const string SignOut = "signout";
            public const string Metadata = "metadata";
        }

        public static class Ids
        {
            private const int WsFederationEventsStart = 50000;

            public const int WsFederationEndpointSuccess = WsFederationEventsStart + 1;
            public const int WsFederationEndpointFailure = WsFederationEventsStart + 2;
        }
    }
}