
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.Api.Services;
using RoundTheCode.Blash.Api.Data;
using Microsoft.Extensions.Options;
using RoundTheCode.Blash.TwitterApi.Models;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using Microsoft.AspNetCore.SignalR;
using RoundTheCode.Blash.Api.Hubs;
using RoundTheCode.Blash.Api.Background.Tasks.BaseObjects;
using TaskExtensions = RoundTheCode.Blash.Api.Background.Tasks.Extensions.TaskExtensions;
using RoundTheCode.Blash.TwitterApi.Services.TweetObjects;
using RoundTheCode.Blash.Api.Extensions;
using RoundTheCode.Blash.Api.Hubs.Extensions;
using RoundTheCode.Blash.Api.Configuration;

namespace RoundTheCode.Blash.Api.Background.Tasks
{
    /// <summary>
    /// Task that imports all recent tweets and adds them to the relevant dashboards.
    /// </summary>
    public class ImportRecentTweetsFromTwitterApiTask : BaseTask<ImportRecentTweetsFromTwitterApiTask>
    {
        protected readonly ITwitterApiTweetService _twitterApiTweetService;
        protected readonly IOptions<ApiConfiguration> _apiConfiguration;
        protected readonly Dashboard _dashboard;
        protected readonly IHubContext<BlashHub> _blashHub;

        /// <summary>
        /// Creates a new instance of <see cref="ImportRecentTweetsFromTwitterApiTask"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used for dependency injection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public ImportRecentTweetsFromTwitterApiTask([NotNull] IServiceProvider serviceProvider, [NotNull] CancellationToken cancellationToken) : base(serviceProvider, cancellationToken)
        {
            _twitterApiTweetService = serviceProvider.GetService<ITwitterApiTweetService>();
            _apiConfiguration = serviceProvider.GetService<IOptions<ApiConfiguration>>();
            _blashHub = serviceProvider.GetService<IHubContext<BlashHub>>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="ImportRecentTweetsFromTwitterApiTask"/>, as well as a dashboard instance, which allows to import recent tweets from a certain dashboard.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used for dependency injection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="dashboard">An instance of a dashboard, so only recent tweets for that dashboard are updated.</param>
        public ImportRecentTweetsFromTwitterApiTask([NotNull] IServiceProvider serviceProvider, [NotNull] CancellationToken cancellationToken, [NotNull] Dashboard dashboard) : this(serviceProvider, cancellationToken)
        {
            _dashboard = dashboard;
        }

        /// <summary>
        /// Performs the recent tweets from the Twitter API for each dashboard.
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

            _logger.LogWithParameters(LogLevel.Information, "Start importing recent tweets from Twitter API", parameters);

            using (var scope = _serviceProvider.CreateScope())
            {
                // Gets the services freom the scope.
                var dashboardService = scope.ServiceProvider.GetService<IDashboardService>();
                var dashboardTweetService = scope.ServiceProvider.GetService<IDashboardTweetService>();
                var authorService = scope.ServiceProvider.GetService<IAuthorService>();
                var tweetService = scope.ServiceProvider.GetService<ITweetService>();
                var blashDbContext = scope.ServiceProvider.GetService<BlashDbContext>();

                var maxResults = _apiConfiguration.Value.Tweets.MaxPerDashboard.HasValue ? _apiConfiguration.Value.Tweets.MaxPerDashboard.Value : 100;

                var updatedTweetIds = new List<int>();

                List<Dashboard> dashboards = null;

                using (var dbContextTransaction = await blashDbContext.Database.BeginTransactionAsync())
                {
                    _logger.LogWithParameters(LogLevel.Debug, "Get all dashboards from database", parameters);
                    dashboards = _dashboard != null ? new List<Dashboard> { _dashboard } : await dashboardService.GetAllAsync(); // Get all dashboards (or one if we have specified a dashboard).
                    _logger.LogWithParameters(LogLevel.Debug, string.Format("{0} dashboard{1} returned from the database", dashboards.Count, dashboards.Count() != 1 ? "s" : ""), parameters);

                    foreach (var dashboard in dashboards)
                    {

                        _logger.LogWithParameters(LogLevel.Information, "Start importing recent tweets for dashboard", parameters);

                        TweetResult recentTweets = null;


                        var recentTweetSearchParameters = new Dictionary<string, object>();
                        foreach (var param in parameters)
                        {
                            recentTweetSearchParameters.Add(param.Key, param.Value);
                        }
                        recentTweetSearchParameters.Add("Search Query", dashboard.SearchQuery);
                        recentTweetSearchParameters.Add("MaxResults", maxResults);

                        // Get recent tweets based on the dashboard's search query.
                        _logger.LogWithParameters(LogLevel.Debug, "Get recent tweets from Twitter API.", recentTweetSearchParameters);
                        try
                        {
                            recentTweets = await _twitterApiTweetService.GetRecentTweetsAsync(dashboard.SearchQuery, maxResults);
                        }
                        catch (Exception)
                        {
                            // Set as null if the API throws an exception.
                            recentTweets = null;
                        }
                        _logger.LogWithParameters(LogLevel.Debug, string.Format("{0} recent tweet{1} returned from the Twitter API.", recentTweets != null && recentTweets.Tweets != null ? recentTweets.Tweets.Count : "0", recentTweets == null || recentTweets.Tweets == null || recentTweets.Tweets.Count() != 1 ? "s" : ""), recentTweetSearchParameters);

                        if (recentTweets == null || recentTweets.Tweets == null || recentTweets.Tweets.Count == 0)
                        {
                            continue;
                        }

                        foreach (var recentTweet in recentTweets.Tweets)
                        {
                            // Go through each tweet returned from the Twitter API.
                            var recentTweetParameters = new Dictionary<string, object>();
                            foreach (var param in recentTweetSearchParameters)
                            {
                                recentTweetParameters.Add(param.Key, param.Value);
                            }
                            recentTweetParameters.Add("Twitter API Tweet Id", recentTweet.Id);

                            _logger.LogWithParameters(LogLevel.Information, "Start importing tweet", recentTweetParameters);

                            if (recentTweet.ReferencedTweets != null && recentTweet.ReferencedTweets.Count > 0)
                            {
                                continue;
                            }

                            // Does tweet exist in the databse.
                            var tweet = await TaskExtensions.CreateUpdateTweetAsync(scope, _logger, recentTweet, recentTweets.Includes.Users, recentTweets.Includes.Media);

                            if (tweet == null)
                            {
                                continue;
                            }

                            // Keep a list of all the updated tweets.
                            updatedTweetIds.Add(tweet.Id);

                            // Update the dashboard & tweet relationship in the database.
                            await TaskExtensions.CreateDashboardTweetRelationshipAsync(scope, _logger, dashboard.Id, tweet.Id);

                            _logger.LogWithParameters(LogLevel.Debug, string.Format("Tweet has been added to Dashboard (id: '{0}').", dashboard.Id), recentTweetParameters);

                            _logger.LogWithParameters(LogLevel.Information, "Finish importing tweet", recentTweetParameters);

                        }

                    }
                    // Commit the transaction.
                    await dbContextTransaction.CommitAsync();
                }


                // Delete any tweets that have not been updated.
                if (_dashboard == null)
                {
                    using (var dbContextTransaction = blashDbContext.Database.BeginTransaction())
                    {
                        await dashboardTweetService.DeleteNonUpdatedTweetsAsync(updatedTweetIds); // Remove non-updated dashboard & tweet relationships.
                        await tweetService.DeleteMissingTweetsFromDashboardAsync(); // Delete any tweets that don't belong to a dashboard.
                        await authorService.DeleteMissingTweetsAsync(); // Delete any authors that don't belong to any tweets.

                        await dbContextTransaction.CommitAsync(); // Commit the transaction to the database.
                    }
                }

                // Send the updated information to the clients connected through SignalR
                await _blashHub.RecentTweetsSyncAsync(await ApiExtensions.GetDashboardAndTweetsAsync(dashboards, tweetService), _dashboard == null, _cancellationToken);
            }


            _logger.LogWithParameters(LogLevel.Information, "Finish importing recent tweets from Twitter API", parameters);

        }
    } 
}
