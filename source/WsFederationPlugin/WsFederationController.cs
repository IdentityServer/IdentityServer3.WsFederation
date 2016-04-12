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

using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.WsFederation.Configuration;
using IdentityServer3.WsFederation.Hosting;
using IdentityServer3.WsFederation.Logging;
using IdentityServer3.WsFederation.ResponseHandling;
using IdentityServer3.WsFederation.Results;
using IdentityServer3.WsFederation.Validation;
using IdentityServer3.WsFederation.Services;
using System;
using System.ComponentModel;
using System.IdentityModel.Services;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

#pragma warning disable 1591

namespace IdentityServer3.WsFederation
{
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [RoutePrefix("")]
    [NoCache]
    [SecurityHeaders(EnableCsp=false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class WsFederationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignInValidator _validator;
        private readonly IRedirectUriValidator _redirectUriValidator;
        private readonly SignOutValidator _signOutValidator;
        private readonly SignInResponseGenerator _signInResponseGenerator;
        private readonly MetadataResponseGenerator _metadataResponseGenerator;
        private readonly ITrackingCookieService _cookies;
        private readonly WsFederationPluginOptions _wsFedOptions;

        public WsFederationController(
            SignInValidator validator,
            SignInResponseGenerator signInResponseGenerator,
            MetadataResponseGenerator metadataResponseGenerator,
            ITrackingCookieService cookies,
            WsFederationPluginOptions wsFedOptions,
            IRedirectUriValidator redirectUriValidator,
            SignOutValidator signOutValidator)
        {
            _validator = validator;
            _signInResponseGenerator = signInResponseGenerator;
            _metadataResponseGenerator = metadataResponseGenerator;
            _cookies = cookies;
            _wsFedOptions = wsFedOptions;
            _redirectUriValidator = redirectUriValidator;
            _signOutValidator = signOutValidator;
        }

        [Route("")]
        [SecurityHeaders(EnableCsp = false, EnableXfo = false)]
        public async Task<IHttpActionResult> Get()
        {
            Logger.Info("Start WS-Federation request");
            Logger.DebugFormat("AbsoluteUri: [{0}]", Request.RequestUri.AbsoluteUri);

            WSFederationMessage message;

            Uri publicRequestUri = GetPublicRequestUri();

            Logger.DebugFormat("PublicUri: [{0}]", publicRequestUri);

            if (WSFederationMessage.TryCreateFromUri(publicRequestUri, out message))
            {
                var signin = message as SignInRequestMessage;
                if (signin != null)
                {
                    Logger.Info("WsFederation signin request");
                    return await ProcessSignInAsync(signin);
                }

                var signout = message as SignOutRequestMessage;
                if (signout != null)
                {
                    Logger.Info("WsFederation signout request");
                    return await ProcessSignOutAsync(signout);
                }
            }

            return BadRequest("Invalid WS-Federation request");
        }

        [Route("signout")]
        [HttpGet]
        public async Task<IHttpActionResult> SignOutCallback()
        {
            Logger.Info("WS-Federation signout callback");

            var urls = await _cookies.GetValuesAndDeleteCookieAsync(WsFederationPluginOptions.CookieName);
            return new SignOutResult(urls);
        }

        [Route("metadata")]
        public IHttpActionResult GetMetadata()
        {
            Logger.Info("WS-Federation metadata request");

            if (_wsFedOptions.EnableMetadataEndpoint == false)
            {
                Logger.Warn("Endpoint is disabled. Aborting.");
                return NotFound();
            }

            var ep = Request.GetOwinContext().Environment.GetIdentityServerBaseUrl() + _wsFedOptions.MapPath.Substring(1);
            var entity = _metadataResponseGenerator.Generate(ep);

            return new MetadataResult(entity);
        }

        private Uri GetPublicRequestUri()
        {
            string identityServerHost = Request.GetOwinContext()
                                               .Environment
                                               .GetIdentityServerHost();

            string pathAndQuery = Request.RequestUri.PathAndQuery;
            string requestUriString = identityServerHost + pathAndQuery;
            var requestUri = new Uri(requestUriString);

            return requestUri;
        }

        private async Task<IHttpActionResult> ProcessSignInAsync(SignInRequestMessage msg)
        {
            var result = await _validator.ValidateAsync(msg, User as ClaimsPrincipal);

            if (result.IsSignInRequired)
            {
                Logger.Info("Redirecting to login page");
                return RedirectToLogin(result);
            }
            if (result.IsError)
            {
                Logger.Error(result.Error);
                return BadRequest(result.Error);
            }

            var responseMessage = await _signInResponseGenerator.GenerateResponseAsync(result);
            await _cookies.AddValueAsync(WsFederationPluginOptions.CookieName, result.ReplyUrl);

            return new SignInResult(responseMessage);
        }

        private async Task<IHttpActionResult> ProcessSignOutAsync(SignOutRequestMessage msg)
        {
            // in order to determine redirect url wreply and wtrealm must be non-empty
            if (String.IsNullOrWhiteSpace(msg.Reply) || String.IsNullOrWhiteSpace(msg.GetParameter("wtrealm")))
            {
                return RedirectToLogOut();
            }

            var result = await _signOutValidator.ValidateAsync(msg);
            if (result.IsError)
            {
                Logger.Error(result.Error);
                return BadRequest(result.Error);
            }

            if (await _redirectUriValidator.IsPostLogoutRedirectUriValidAsync(msg.Reply, result.RelyingParty) == false)
            {
                const string error = "invalid_signout_reply_uri";
                Logger.Error(error);
                return BadRequest(error);
            }

            return RedirectToLogOut(msg.Reply);
        }

        IHttpActionResult RedirectToLogin(SignInValidationResult result)
        {
            Uri publicRequestUri = GetPublicRequestUri();

            var message = new SignInMessage();
            message.ReturnUrl = publicRequestUri.AbsoluteUri;

            if (!String.IsNullOrWhiteSpace(result.HomeRealm))
            {
                message.IdP = result.HomeRealm;
            }

            if (!String.IsNullOrWhiteSpace(result.Federation))
            {
                message.AcrValues = new[] { result.Federation };
            }
            
            var env = Request.GetOwinEnvironment();
            var url = env.CreateSignInRequest(message);
            
            return Redirect(url);
        }

        IHttpActionResult RedirectToLogOut()
        {
            return Redirect(Request.GetOwinEnvironment().GetIdentityServerLogoutUrl());
        }

        IHttpActionResult RedirectToLogOut(string returnUrl)
        {
            var message = new SignOutMessage
            {
                ReturnUrl = returnUrl
            };

            var env = Request.GetOwinEnvironment();
            var url = env.CreateSignOutRequest(message);

            return Redirect(url);
        }
    }
}
