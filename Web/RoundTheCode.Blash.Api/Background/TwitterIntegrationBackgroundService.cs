using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using RoundTheCode.Blash.TwitterApi.Configuration;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.Api.Background.Tasks;
using RoundTheCode.Blash.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using RoundTheCode.Blash.Api.Background.Jobs;
using RoundTheCode.Blash.Api.Configuration;

namespace RoundTheCode.Blash.Api.Background
{
    /// <summary>
    /// A hosted service, which creates tasks and runs them through the job service. 
    /// </summary>
    public class TwitterIntegrationBackgroundService : IHostedService
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger<TwitterIntegrationBackgroundService> _logger;
        protected readonly IOptions<ApiConfiguration> _apiConfiguration;
        protected readonly IHubContext<BlashHub> _blashHub;
        protected readonly ITwitterIntegrationJobService _twitterIntegrationJobService;
        protected readonly IHostApplicationLifetime _hostApplicationLifetime;

        protected ListenForRealtimeTweetsFromTwitterApiTask ListenForRealtimeTweetsFromTwitterApiTask { get; set; }
        protected ImportRecentTweetsFromTwitterApiTask ImportRecentTweetsFromTwitterApiTask { get; set; }


        /// <summary>
        /// Creates a new instance of <see cref="TwitterIntegrationBackgroundService"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used for dependency injection.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        /// <param name="apiConfiguration">API configuration settings, used in appsettings.json under the 'Api' key.</param>
        /// <param name="blashHub">The SignalR chat hub context.</param>
        /// <param name="twitterIntegrationJobService">An instance of the job service, so we can run the tasks.</param>
        /// <param name="hostApplicationLifetime">An instance of the lifetime, so we can get the cancellation token.</param>
        public TwitterIntegrationBackgroundService([NotNull] IServiceProvider serviceProvider,
            [NotNull] ILogger<TwitterIntegrationBackgroundService> logger, [NotNull] IOptions<ApiConfiguration> apiConfiguration,
            [NotNull] IHubContext<BlashHub> blashHub,
            [NotNull] ITwitterIntegrationJobService twitterIntegrationJobService,
            [NotNull] IHostApplicationLifetime hostApplicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _apiConfiguration = apiConfiguration;
            _logger = logger;
            _blashHub = blashHub;
            _twitterIntegrationJobService = twitterIntegrationJobService;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        /// <summary>
        /// Starts the hosted service, running the tasks needed before the API can properly initalise.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "StartAsync");


            _logger.LogWithParameters(LogLevel.Information, "Starting Twitter Integration Background Service...", parameters);

            // Create Tasks
            var importRulesFromTwitterTask = new ImportRulesFromTwitterApiTask(_serviceProvider, _hostApplicationLifetime.ApplicationStopping);
            ImportRecentTweetsFromTwitterApiTask = new ImportRecentTweetsFromTwitterApiTask(_serviceProvider, _hostApplicationLifetime.ApplicationStopping);
            ListenForRealtimeTweetsFromTwitterApiTask = new ListenForRealtimeTweetsFromTwitterApiTask(_serviceProvider, _hostApplicationLifetime.ApplicationStopping);

            // Process Tweet when Recieved
            ListenForRealtimeTweetsFromTwitterApiTask.OnTweetReceived += async (tweetResponse) =>
            {
                // When a tweet is received run the 'OnRealtimeTweetReceivedTask' task.
                await _twitterIntegrationJobService.RunJobAsync(new TwitterIntegrationJob((Guid id) => new OnRealtimeTweetReceivedTask(_serviceProvider, _hostApplicationLifetime.ApplicationStopping, tweetResponse).RunAsync(id)));
            };

            // Run Tasks for importing rules, listening for realtime tweets, and importing recent tweets.
            await _twitterIntegrationJobService.RunJobAsync(new TwitterIntegrationJob((Guid id) => importRulesFromTwitterTask.RunAsync(id)));
            await _twitterIntegrationJobService.RunJobAsync(new TwitterIntegrationJob((Guid id) => ListenForRealtimeTweetsFromTwitterApiTask.RunAsync(id)));
            await _twitterIntegrationJobService.RunJobAsync(new TwitterIntegrationJob((Guid id) => ImportRecentTweetsFromTwitterApiTask.RunAsync(id)));

            // Schedule a task to import for the recent tweets at regular intervals.
            SetUpSchedule();

            _logger.LogWithParameters(LogLevel.Information, "Started Twitter Integration Background Service", parameters);

     
        }

        /// <summary>
        /// Stops the hosted service.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "StopAsync");

            _logger.LogWithParameters(LogLevel.Information, "Stopping Twitter Integration Background Service...", parameters);

            var task = Task.CompletedTask;
            _logger.LogWithParameters(LogLevel.Information, "Stopped Twitter Integration Background Service", parameters);

            return task;
        }

        /// <summary>
        /// Set up regular schedule for running jobs.
        /// </summary>
        private void SetUpSchedule()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "SetUpSchedule");

            // Run async, so we are not holding up the ability to start the hosted service.
            Task.Run(async () =>
            {
                // Continue to run the task until cancellation is requested.
                while (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    // Get the recent tweets schedule from Api > Tweets > SyncRecentSchedule in appsettings.json.
                    var importRecentTweetsSchedule = _apiConfiguration.Value.Tweets.SyncRecentSchedule.HasValue ? _apiConfiguration.Value.Tweets.SyncRecentSchedule.Value : new TimeSpan(0, 5, 0);

                    var nextSchedule = Task.Delay(importRecentTweetsSchedule, _hostApplicationLifetime.ApplicationStopping); // Delay the schedule until the sync recent schedule has been reached.
                    _logger.LogWithParameters(LogLevel.Information, string.Format("Set up schedule for importing recent tweets. The importing of recent tweets will happen every {0}", importRecentTweetsSchedule.ToString("h\\:mm\\:ss")), parameters);
                    await nextSchedule;

                    // Import recent tweets from the Twitter API.
                    ImportRecentTweetsFromTwitterApiTask = new ImportRecentTweetsFromTwitterApiTask(_serviceProvider, _hostApplicationLifetime.ApplicationStopping);
                    await _twitterIntegrationJobService.RunJobAsync(new TwitterIntegrationJob((Guid id) => ImportRecentTweetsFromTwitterApiTask.RunAsync(id)));
                };
            });
        }
    }
}
