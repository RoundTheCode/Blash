using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.TwitterApi.Configuration;
using RoundTheCode.Blash.TwitterApi.Exceptions;
using RoundTheCode.Blash.TwitterApi.Models;
using RoundTheCode.Blash.TwitterApi.Services.AuthenticateObjects;
using RoundTheCode.Blash.TwitterApi.Services.Extensions;

namespace RoundTheCode.Blash.TwitterApi.Services.TweetObjects
{
    /// <summary>
    /// A service for Twitter API tweets.
    /// </summary>
    public class TwitterApiTweetService : ITwitterApiTweetService
    {
        protected readonly ITwitterApiAuthenticateService _twitterApiAuthenticateService;
        protected readonly IOptions<TwitterApiConfiguration> _twitterApiConfiguration;
        protected readonly ILogger<TwitterApiTweetService> _logger;

        // Details what tweet data to return from the Twitter API.
        const string EXPANSIONS_AND_FIELD_QUERY = "expansions=author_id,in_reply_to_user_id,attachments.media_keys&user.fields=profile_image_url,username&tweet.fields=created_at,referenced_tweets&media.fields=media_key,url,type";

        /// <summary>
        /// Creates an instance of <see cref="TwitterApiTweetService"/>.
        /// </summary>
        /// <param name="twitterApiAuthenticateService">Used to get information to authenticate against the Twitter API.</param>
        /// <param name="twitterApiConfiguration">Used to retrieve authorisation information about the Twitter API, like the client ID and secret.</param>
        /// <param name="logger">Used to log outputs.</param>
        public TwitterApiTweetService([NotNull] ITwitterApiAuthenticateService twitterApiAuthenticateService, [NotNull] IOptions<TwitterApiConfiguration> twitterApiConfiguration, [NotNull] ILogger<TwitterApiTweetService> logger)
        {
            _twitterApiAuthenticateService = twitterApiAuthenticateService;
            _twitterApiConfiguration = twitterApiConfiguration;
            _logger = logger;
        }

        public async Task<TweetResult> GetRecentTweetsAsync(string query, int maxResults)
        {
            var url = string.Format("https://api.twitter.com/2/tweets/search/recent?query={0}&max_results={1}&{2}", HttpUtility.UrlPathEncode(query), maxResults, EXPANSIONS_AND_FIELD_QUERY);

            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetRecentTweetsAsync");
            parameters.Add("Uri", url);
            parameters.Add("MaxRules", maxResults);

            try
            {

                TweetResult tweetResult = null;

                using (var httpClient = new HttpClient())
                {
                    // Calls a GET request.

                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        request.Headers.Add("Authorization", string.Format("Bearer {0}", await _twitterApiAuthenticateService.GetOAuth2TokenAsync()));  // Uses oauth authorisation, with the bearer token from the Twitter API.

                        using (var response = await TwitterApiServiceExtensions.GetTwitterApiResponseAsync(httpClient, request))
                        {
                            // Reads the response from the HTTP request and gets the content.
                            var content = await response.Content.ReadAsStringAsync();

                            try
                            {
                                // Converts the response to a TweetResult instance.
                                tweetResult = JsonConvert.DeserializeObject<TweetResult>(content);
                            }
                            catch (Exception exception)
                            {
                                // Throw an error if unable to convert to a TweetResult.
                                throw new TwitterApiException("Unable to convert the JSON response to a RuleResult type.", exception);
                            }
                        }
                    }
                }

                return tweetResult;
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
        /// Begins the Twitter API search stream, allowing to retrieve tweets in real time.
        /// </summary>
        /// <returns>The response from the HTTP request.</returns>
        public async Task<HttpResponseMessage> GetTweetsSearchStreamResponseAsync()
        {
            var url = string.Format("https://api.twitter.com/2/tweets/search/stream?{0}", EXPANSIONS_AND_FIELD_QUERY);

            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetTweetsSearchStreamResponseAsync");
            parameters.Add("Uri", url);

            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        request.Headers.Add("Authorization", string.Format("Bearer {0}", await _twitterApiAuthenticateService.GetOAuth2TokenAsync())); // Uses oauth authorisation, with the bearer token from the Twitter API.

                        return await TwitterApiServiceExtensions.GetTwitterApiResponseAsync(httpClient, request); // Returns the response from the HTTP request.
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

        /// <summary>
        /// Called to listen to the Twitter API search stream.
        /// </summary>
        /// <param name="response">The response when beginning the Twitter API search stream.</param>
        /// <param name="onStreamResponse">A delegate that can be used when a response is returned from the stream.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task GetTweetsSearchStreamAsync(HttpResponseMessage response, Action<string, ILogger, Dictionary<string, object>> onStreamResponse)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetTweetsSearchStreamAsync");
            try
            {
                // Start the stream.
                using (var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                {
                    while (!reader.EndOfStream)
                    {
                        //We are ready to read the stream
                        var line = await reader.ReadLineAsync();

                        // An output will happen roughly every 20 seconds to keep the stream alive.
                        // Need to check if the link is not empty.

                        if (!string.IsNullOrWhiteSpace(line)) 
                        {
                            // Invoke the onStreamResponse delegate if the line is not empty.
                            onStreamResponse?.Invoke(line, _logger, parameters);
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
            finally
            {
                // Dispose of the response if the stream ends.
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }
    }
}
