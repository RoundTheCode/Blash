using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using RoundTheCode.Blash.TwitterApi.Exceptions;
using System.Net.Http;
using System.Threading.Tasks;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.Api.Background.Tasks.BaseObjects;
using RoundTheCode.Blash.TwitterApi.Services.TweetObjects;

namespace RoundTheCode.Blash.Api.Background.Tasks
{
    /// <summary>
    /// Listens to any tweets that come through from the Twitter API's recent tweet stream.
    /// </summary>
    public class ListenForRealtimeTweetsFromTwitterApiTask : BaseTask<ListenForRealtimeTweetsFromTwitterApiTask>
    {
        protected readonly ITwitterApiTweetService _twitterApiTweetService;

        /// <summary>
        /// Event delegate for when a tweet is received from the stream.
        /// </summary>
        public event Action<string> OnTweetReceived;

        /// <summary>
        /// Creates a new instance of <see cref="ListenForRealtimeTweetsFromTwitterApiTask"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used for dependency injection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public ListenForRealtimeTweetsFromTwitterApiTask([NotNull] IServiceProvider serviceProvider, [NotNull] CancellationToken cancellationToken) : base(serviceProvider, cancellationToken)
        {
            _twitterApiTweetService = serviceProvider.GetService<ITwitterApiTweetService>();
        }

        /// <summary>
        /// Performs the task to listen to realtime tweets from the Twitter API.
        /// </summary>
        /// <param name="id">The job identifier.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        protected override async Task TaskActionAsync(Guid? id)
        {            
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "TaskActionAsync");
            if (id.HasValue)
            {
                parameters.Add("Job ID", id);
            }

            try
            {
                if (!_cancellationToken.IsCancellationRequested)
                {
                    // Search the stream, getting the HTTP response from the Tweet Search Stream Request in the Twitter API.
                    SearchStream(id, await _twitterApiTweetService.GetTweetsSearchStreamResponseAsync());
                }
            }
            catch (TwitterApiException exception)
            {
                // If exception is thrown, attempt to reconnect to stream.
                ReconnectToStream(id, exception.XRateLimitResetDate);
            }
            catch (Exception)
            {
                // If exception is thrown, attempt to reconnect to stream.
                ReconnectToStream(id, null);
            }
        }

        protected void SearchStream(Guid? id, HttpResponseMessage response)
        {           
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "SearchStream");
            if (id.HasValue)
            {
                parameters.Add("Job ID", id);
            }

            // Run the job async, as the stream will never return from this method whilst it's running.
            Task.Run(async() =>
            {
                try
                {
                    _logger.LogWithParameters(LogLevel.Information, "Start importing realtime tweets from Twitter API", parameters);

                    // Listen to the search stream.
                    await _twitterApiTweetService.GetTweetsSearchStreamAsync(response, (realTimeTweet, logger, parameters) =>
                    {
                        // When a tweet is received from the search stream.
                        if (OnTweetReceived != null)
                        {
                            // Invoke the OnTweetReceived delegate, passing in a JSON string of the tweet from the Twitter API.
                            OnTweetReceived.Invoke(realTimeTweet);
                        }
                    });

                    _logger.LogWithParameters(LogLevel.Warning, "Finish importing realtime tweets from Twitter API", parameters);
                }
                catch (TwitterApiException exception)
                {
                    // If the stream disconnects, log the exception and attempt to reconnect to it.
                    _logger.LogWithParameters(LogLevel.Error, exception, "Disconnected from the Twitter API", parameters);
                    ReconnectToStream(id, exception.XRateLimitResetDate);
                }
                catch (Exception exception)
                {
                    // If the stream disconnects, log the exception and attempt to reconnect to it.
                    _logger.LogWithParameters(LogLevel.Error, exception, "Disconnected from the Twitter API", parameters);
                    ReconnectToStream(id, null);
                }
            }, _cancellationToken);

        }

        protected void ReconnectToStream(Guid? id, DateTimeOffset? reconnectTime)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "ReconnectToStream");
            if (id.HasValue)
            {
                parameters.Add("Job ID", id);
            }

            // Run the task to reconnect async, as there will be a time delay before it attempts to reconnect to the Twitter stream.
            Task.Run(async () =>
            {
                var interval = reconnectTime.HasValue ? reconnectTime.Value.Subtract(DateTimeOffset.Now) : new TimeSpan(0, 1, 0); // If a reconnect time is supplied, delay the reconnection of the stream until the reconnect time has been reached.

                _logger.LogWithParameters(LogLevel.Information, string.Format("Wait {0} before trying again.", interval.ToString("h\\:mm\\:ss")), parameters);
                await Task.Delay(interval, _cancellationToken); // Delay before reconnecting to the stream.
                await RunAsync(id); // Re-run the task.
            }, _cancellationToken);
        }

    }
}
