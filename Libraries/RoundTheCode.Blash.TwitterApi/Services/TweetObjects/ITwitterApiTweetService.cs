using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RoundTheCode.Blash.TwitterApi.Models;

namespace RoundTheCode.Blash.TwitterApi.Services.TweetObjects
{
    /// <summary>
    /// A service for Twitter API tweets.
    /// </summary>
    public interface ITwitterApiTweetService
    {
        /// <summary>
        /// Get a list of recent tweets.
        /// </summary>
        /// <param name="query">The query used against the tweets.</param>
        /// <param name="maxResults">The maximum number of results retunred.</param>
        /// <returns>An instance of <see cref="TweetResult"/>, which lists all the tweets that match the query.</returns>
        Task<TweetResult> GetRecentTweetsAsync(string query, int maxResults);

        /// <summary>
        /// Begins the Twitter API search stream, allowing to retrieve tweets in real time.
        /// </summary>
        /// <returns>The response from the HTTP request.</returns>
        Task<HttpResponseMessage> GetTweetsSearchStreamResponseAsync();

        /// <summary>
        /// Called to listen to the Twitter API search stream.
        /// </summary>
        /// <param name="response">The response when beginning the Twitter API search stream.</param>
        /// <param name="onStreamResponse">A delegate that can be used when a response is returned from the stream.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task GetTweetsSearchStreamAsync(HttpResponseMessage response, Action<string, ILogger, Dictionary<string, object>> onStreamResponse);
    }
}
