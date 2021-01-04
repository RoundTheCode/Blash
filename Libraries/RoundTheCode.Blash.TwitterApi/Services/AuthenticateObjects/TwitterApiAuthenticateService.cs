using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.TwitterApi.Configuration;
using RoundTheCode.Blash.TwitterApi.Exceptions;
using RoundTheCode.Blash.TwitterApi.Extensions;
using RoundTheCode.Blash.TwitterApi.Services.Extensions;

namespace RoundTheCode.Blash.TwitterApi.Services.AuthenticateObjects
{
    /// <summary>
    /// A service for Twitter API authentication.
    /// </summary>
    public class TwitterApiAuthenticateService : ITwitterApiAuthenticateService
    {
        protected readonly ILogger<TwitterApiAuthenticateService> _logger;
        protected readonly IOptions<TwitterApiConfiguration> _twitterApiConfiguration;

        /// <summary>
        /// Creates an instance of <see cref="TwitterApiAuthenticateService"/>, passing in the logger and the Twitter API configuration.
        /// </summary>
        /// <param name="logger">Used to log outputs.</param>
        /// <param name="twitterApiConfiguration">Used to retrieve authorisation information about the Twitter API, like the client ID and secret.</param>
        public TwitterApiAuthenticateService([NotNull] ILogger<TwitterApiAuthenticateService> logger, [NotNull] IOptions<TwitterApiConfiguration> twitterApiConfiguration)
        {
            _logger = logger;
            _twitterApiConfiguration = twitterApiConfiguration;
        }

        /// <summary>
        /// Gets the OAuth2 token which can be used when calling Twitter API methods.
        /// </summary>
        /// <returns>The OAuth2 token.</returns>
        public async Task<string> GetOAuth2TokenAsync()
        {
            var url = "https://api.twitter.com/oauth2/token";

            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetOAuth2TokenAsync");
            parameters.Add("Uri", url);

            var content = string.Empty;

            try
            {
                string token = string.Empty;
                using (var httpClient = new HttpClient())
                {
                    // Calls a POST request.
                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Headers.Add("Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", _twitterApiConfiguration.Value.ClientId, _twitterApiConfiguration.Value.ClientSecret))))); // Uses basic authorisation, with the client ID as the username, and the client secret as the password.
                        request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" } }); // Pass in a form value of "grant_type".

                        // Retrieve the response.
                        using (var response = await TwitterApiServiceExtensions.GetTwitterApiResponseAsync(httpClient, request))
                        {                           
                            var jsonContent = JObject.Parse(await response.Content.ReadAsStringAsync());
                            token = jsonContent["access_token"].ToString(); // Get the access token from the response.
                        }
                    }
                }
                return token;
            }
            catch (TwitterApiException exception)
            {
                // Exception thrown from the Twitter API. Log the error.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
            catch (Exception exception)
            {
                // General exception thrown. Log the error.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }



        }
    }
}
