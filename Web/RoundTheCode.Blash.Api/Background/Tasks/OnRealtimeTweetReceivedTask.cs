using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Exceptions;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RoundTheCode.Blash.TwitterApi.Models;
using RoundTheCode.Blash.Api.Services;
using RoundTheCode.Blash.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.Api.Background.Tasks.BaseObjects;
using TaskExtensions = RoundTheCode.Blash.Api.Background.Tasks.Extensions.TaskExtensions;
using RoundTheCode.Blash.Data.Results.CreateTweetObjects;
using RoundTheCode.Blash.Api.Hubs.Extensions;
using RoundTheCode.Blash.Api.Data;
using Microsoft.Extensions.Options;
using RoundTheCode.Blash.TwitterApi.Configuration;
using System.Linq;
using RoundTheCode.Blash.Api.Configuration;

namespace RoundTheCode.Blash.Api.Background.Tasks
{
    /// <summary>
    /// What to do when a tweet is received from the Twitter API.
    /// </summary>
    public class OnRealtimeTweetReceivedTask : BaseTask<OnRealtimeTweetReceivedTask>
    {
        protected readonly string _tweetResponse;
        protected readonly IHubContext<BlashHub> _blashHub;
        protected readonly IOptions<ApiConfiguration> _apiConfiguration;

        /// <summary>
        /// Creates a new instance of <see cref="OnRealtimeTweetReceivedTask"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used for dependency injection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="tweetResponse">The tweet response received from the Twitter API.</param>
        public OnRealtimeTweetReceivedTask([NotNull] IServiceProvider serviceProvider, [NotNull] CancellationToken cancellationToken, string tweetResponse) : base(serviceProvider, cancellationToken)
        {
            _tweetResponse = tweetResponse;
            _blashHub = serviceProvider.GetService<IHubContext<BlashHub>>();
            _apiConfiguration = serviceProvider.GetService<IOptions<ApiConfiguration>>();
        }

        protected override async Task TaskActionAsync(Guid? id)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "TaskActionAsync");
            if (id.HasValue)
            {
                parameters.Add("Job ID", id);
            }

            //  Attempt to convert the tweet response JSON string into a SearchStreamResult object.
            SearchStreamResult searchStreamResult;
            try
            {
                searchStreamResult = JsonConvert.DeserializeObject<SearchStreamResult>(_tweetResponse);
            }
            catch (Exception exception)
            {
                // Throws exception, if it's unable to convert it into a SearchStreamResult object.
                throw new ApiException("Unable to convert JSON to type SearchStreamResult.", exception, JObject.Parse(_tweetResponse));
            }

            if (searchStreamResult != null && searchStreamResult.Tweet == null)
            {
                // No tweet, so throw an exception.
                throw new ApiException("Tweet does not have an instance of Tweet.", JObject.Parse(_tweetResponse));
            }

            if (searchStreamResult != null && searchStreamResult.Tweet != null && (searchStreamResult.Tweet.ReferencedTweets == null || searchStreamResult.Tweet.ReferencedTweets.Count == 0))
            {
                var tweetParameters = new Dictionary<string, object>();
                foreach (var param in parameters)
                {
                    tweetParameters.Add(param.Key, param.Value);
                }
                tweetParameters.Add("Twitter API Tweet Id", searchStreamResult.Tweet.Id);

                _logger.LogWithParameters(LogLevel.Information, "Start importing incoming realtime tweet", tweetParameters);

                Tweet tweet = null;
                List<DashboardTweet> dashboardTweets = null;

                // Create a new scope from the service provider.
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Gets the services freom the scope.
                    var blashDbContext = scope.ServiceProvider.GetService<BlashDbContext>();
                    var dashboardService = scope.ServiceProvider.GetService<IDashboardService>();
                    var dashboardTweetService = scope.ServiceProvider.GetService<IDashboardTweetService>();
                    var tweetService = scope.ServiceProvider.GetService<ITweetService>();
                    var authorService = scope.ServiceProvider.GetService<IAuthorService>();

                    var dashboards = new List<Dashboard>();
                    IList<DashboardTweet> dashboardTweetsToDelete = null;

                    using (var dbContextTransaction = await blashDbContext.Database.BeginTransactionAsync())
                    {
                        // Go through each rule that matches a tweet from the Twitter API.
                        foreach (var matchingRule in searchStreamResult.MatchingRules)
                        {
                            var dashboard = await dashboardService.GetByTwitterRuleAsync(matchingRule.Id); // Dashboard may have been deleted, so check it still exists.

                            if (dashboard == null)
                            {
                                // No dashboard exists, so continue with the next rule.
                                _logger.LogWithParameters(LogLevel.Information, "Dashboard no longer exists so finish processing.", tweetParameters);
                                continue;
                            }
                            dashboards.Add(dashboard); // Add to the list of dashboards.
                        }

                        if (dashboards == null || dashboards.Count == 0)
                        {
                            // If there are no dashboards assoicated with the tweet, finish with the task.
                            return;
                        }

                        // Create or update the tweet to the Blash database.
                        tweet = await TaskExtensions.CreateUpdateTweetAsync(scope, _logger, searchStreamResult.Tweet, searchStreamResult.Includes.Users, searchStreamResult.Includes.Media);

                        if (tweet == null)
                        {
                            return;
                        }

                        // Add it into a dashboard/tweet relationship.
                        dashboardTweets = null;

                        foreach (var dashboard in dashboards)
                        {
                            if (dashboardTweets == null)
                            {
                                dashboardTweets = new List<DashboardTweet>();
                            }

                            // Add all the relationships associated with the tweet.
                            dashboardTweets.Add(await TaskExtensions.CreateDashboardTweetRelationshipAsync(scope, _logger, dashboard.Id, tweet.Id));

                            // Ensure that we only have have the maximum number of tweets per dashboard as defined in appsettings.json under Api > Tweets > MaxPerDashboard.
                            dashboardTweetsToDelete = await dashboardTweetService.GetByDashboardAndAfterPositionAsync(dashboard.Id, _apiConfiguration.Value.Tweets?.MaxPerDashboard ?? 10);

                            if (dashboardTweetsToDelete != null && dashboardTweetsToDelete.Count > 0)
                            {
                                // Delete any dashboard/tweets that exceed the maximum number of tweets allowed for a dashboard.
                                await dashboardTweetService.DeleteMultipleAsync(dashboardTweetsToDelete.Select(dashboardTweet => dashboardTweet.Id).ToList());
                            }

                            // Make sure tweets are removed that aren't needed.
                            await tweetService.DeleteMissingTweetsFromDashboardAsync();
                            await authorService.DeleteMissingTweetsAsync();
                        }

                        // Commit the transaction to the database.
                        await dbContextTransaction.CommitAsync();
                    }

                    // Send the fact that a tweet has been created to the hub's connected clinets through SignalR.
                    await _blashHub.CreateTweetAsync(new CreateTweetResult { Data = new List<CreateTweetData> { new CreateTweetData(tweet, dashboardTweets) } }, _cancellationToken);

                    if (dashboardTweetsToDelete != null && dashboardTweetsToDelete.Count > 0)
                    {
                        // Send the fact that tweets have been deleted from dashboards to the hub's connected clinets through SignalR.
                        await _blashHub.DeleteDashboardTweetAsync(dashboardTweetsToDelete, _cancellationToken);
                    }
                }

                _logger.LogWithParameters(LogLevel.Information, "Finish importing incoming realtime tweet", tweetParameters);
            }
        }

    }
}
