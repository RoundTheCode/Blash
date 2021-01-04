using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.TwitterApi.Configuration;
using RoundTheCode.Blash.TwitterApi.Data.RuleObjects;
using RoundTheCode.Blash.TwitterApi.Exceptions;
using RoundTheCode.Blash.TwitterApi.Results.RuleObjects;
using RoundTheCode.Blash.TwitterApi.Services.AuthenticateObjects;
using RoundTheCode.Blash.TwitterApi.Services.Extensions;

namespace RoundTheCode.Blash.TwitterApi.Services.RuleObjects
{
    /// <summary>
    /// A service for Twitter API rules.
    /// </summary>
    public class TwitterApiRuleService : ITwitterApiRuleService
    {
        protected readonly ITwitterApiAuthenticateService _twitterApiAuthenticateService;
        protected readonly IOptions<TwitterApiConfiguration> _twitterApiConfiguration;
        protected readonly ILogger<TwitterApiRuleService> _logger;

        /// <summary>
        /// Creates an instance of <see cref="TwitterApiRuleService"/>.
        /// </summary>
        /// <param name="twitterApiAuthenticateService">Used to get information to authenticate against the Twitter API.</param>
        /// <param name="twitterApiConfiguration">Used to retrieve authorisation information about the Twitter API, like the client ID and secret.</param>
        /// <param name="logger">Used to log outputs.</param>
        public TwitterApiRuleService([NotNull] ITwitterApiAuthenticateService twitterApiAuthenticateService, [NotNull] IOptions<TwitterApiConfiguration> twitterApiConfiguration, [NotNull] ILogger<TwitterApiRuleService> logger)
        {
            _twitterApiAuthenticateService = twitterApiAuthenticateService;
            _twitterApiConfiguration = twitterApiConfiguration;
            _logger = logger;
        }

        /// <summary>
        /// Method to create a Twitter API rule.
        /// </summary>
        /// <param name="ruleEntries">A list of rule entries to be created.</param>
        /// <returns>An instance of <see cref="RuleResult"/>, which lists all the rules created, along with their unique identifier.</returns>
        public async Task<RuleResult> CreateRuleAsync(List<RuleEntry> ruleEntries)
        {
            var url = "https://api.twitter.com/2/tweets/search/stream/rules";

            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "CreateRuleAsync");
            parameters.Add("Uri", url);

            try
            {
                RuleResult ruleResult = null;

                using (var httpClient = new HttpClient())
                {
                    // Calls a POST request.
                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Headers.Add("Authorization", string.Format("Bearer {0}", await _twitterApiAuthenticateService.GetOAuth2TokenAsync())); // Uses oauth authorisation, with the bearer token from the Twitter API.
                        request.Content = new StringContent(JsonConvert.SerializeObject(new { add = ruleEntries }), Encoding.UTF8, "application/json"); // Pass the list of rules to add in JSON format.
                        
                        // Retrieve the response.
                        using (var response = await TwitterApiServiceExtensions.GetTwitterApiResponseAsync(httpClient, request))
                        {
                            // Reads the content from the response.
                            var content = await response.Content.ReadAsStringAsync();

                            try
                            {
                                // Converts the response to a RuleResult instance.
                                ruleResult = JsonConvert.DeserializeObject<RuleResult>(content);
                            }
                            catch (Exception exception)
                            {
                                // Throw an error if unable to convert to a RuleResult.
                                throw new TwitterApiException("Unable to convert the JSON response to a RuleResult type.", exception);
                            }
                        }
                    }
                }

                return ruleResult;
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

        /// <summary>
        /// Get a list of all the Twitter API rules.
        /// </summary>
        /// <returns>An instance of <see cref="RuleResult"/>, which lists all the rules.</returns>
        public async Task<RuleResult> GetStreamRulesAsync()
        {
            var url = "https://api.twitter.com/2/tweets/search/stream/rules";

            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetStreamRulesAsync");
            parameters.Add("Uri", url);

            try
            {
                RuleResult ruleResult = null;

                using (var httpClient = new HttpClient())
                {
                    // Calls a GET request.

                    using (var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitter.com/2/tweets/search/stream/rules"))
                    {
                        request.Headers.Add("Authorization", string.Format("Bearer {0}", await _twitterApiAuthenticateService.GetOAuth2TokenAsync())); // Uses oauth authorisation, with the bearer token from the Twitter API.

                        // Retrieve the response.
                        using (var response = await TwitterApiServiceExtensions.GetTwitterApiResponseAsync(httpClient, request))
                        {
                            // Reads the content from the response.
                            var content = await response.Content.ReadAsStringAsync();

                            try
                            {
                                // Converts the response to a RuleResult instance.
                                ruleResult = JsonConvert.DeserializeObject<RuleResult>(content);
                            }
                            catch (Exception exception)
                            {
                                // Throw an error if unable to convert to a RuleResult.
                                throw new TwitterApiException("Unable to convert the JSON response to a RuleResult type.", exception);
                            }
                        }

                    }
                }

                return ruleResult;
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

        /// <summary>
        /// Deletes rules from the Twitter API.
        /// </summary>
        /// <param name="ruleIds">A list of all the rule IDs to be deleted.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteRuleAsync(List<string> ruleIds)
        {
            var url = "https://api.twitter.com/2/tweets/search/stream/rules";

            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteRuleAsync");
            parameters.Add("Uri", url);

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Calls a POST request.
                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Headers.Add("Authorization", string.Format("Bearer {0}", await _twitterApiAuthenticateService.GetOAuth2TokenAsync()));  // Uses oauth authorisation, with the bearer token from the Twitter API.
                        request.Content = new StringContent(JsonConvert.SerializeObject(new { delete = new { ids = ruleIds } }), Encoding.UTF8, "application/json"); // Pass the list of rules to delete in JSON format.

                        // Executes the response.
                        using (var response = await TwitterApiServiceExtensions.GetTwitterApiResponseAsync(httpClient, request))
                        {
                        }
                    }
                }
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
