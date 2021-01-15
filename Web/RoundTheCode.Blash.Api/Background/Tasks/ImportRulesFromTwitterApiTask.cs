using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Data;
using RoundTheCode.Blash.Api.Services;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.TwitterApi.Services;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.Api.Background.Tasks.BaseObjects;
using RoundTheCode.Blash.TwitterApi.Services.RuleObjects;

namespace RoundTheCode.Blash.Api.Background.Tasks
{
    /// <summary>
    /// Task that imports all the rules, and creates dashboards as a result of them.
    /// </summary>
    public class ImportRulesFromTwitterApiTask : BaseTask<ImportRulesFromTwitterApiTask>
    {
        protected readonly ITwitterApiRuleService _twitterApiRuleService;

        /// <summary>
        /// Creates a new instance of <see cref="ImportRulesFromTwitterApiTask"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used for dependency injection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public ImportRulesFromTwitterApiTask([NotNull] IServiceProvider serviceProvider, [NotNull] CancellationToken cancellationToken) : base(serviceProvider, cancellationToken)
        {
            _twitterApiRuleService = serviceProvider.GetService<ITwitterApiRuleService>();
        }

        /// <summary>
        /// Performs the task to import rules from the Twitter API.
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

            _logger.LogWithParameters(LogLevel.Information, "Start importing rules from Twitter API", parameters);

            if (!_cancellationToken.IsCancellationRequested)
            {
                // Get rules from the Twitter API
                _logger.LogWithParameters(LogLevel.Debug, "Get Stream Rules from Twitter API", parameters);
                var rules = await _twitterApiRuleService.GetStreamRulesAsync();
                _logger.LogWithParameters(LogLevel.Debug, string.Format("{0} rule{1} returned from Twitter API", (rules.RuleEntries != null ? rules.RuleEntries.Count : 0), rules.RuleEntries == null || rules.RuleEntries.Count() != 1 ? "s" : ""), parameters);



                // Create a new scope from the service provider.
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Gets the services freom the scope.
                    var dashboardService = scope.ServiceProvider.GetService<IDashboardService>();
                    var dashboardTweetService = scope.ServiceProvider.GetService<IDashboardTweetService>();
                    var tweetService = scope.ServiceProvider.GetService<ITweetService>();
                    var authorService = scope.ServiceProvider.GetService<IAuthorService>();
                    var blashDbContext = scope.ServiceProvider.GetService<BlashDbContext>();

                    var order = 0;
                    var updatedDashboardIds = new List<int>();

                    // For each rule, create or update it to the database.
                    if (rules.RuleEntries != null)
                    {
                        foreach (var ruleEntry in rules.RuleEntries)
                        {
                            var dashboardParameters = new Dictionary<string, object>();
                            foreach (var param in parameters)
                            {
                                dashboardParameters.Add(param.Key, param.Value);
                            }
                            dashboardParameters.Add("Twitter API Rule ID", ruleEntry.Id);

                            _logger.LogWithParameters(LogLevel.Information, "Start Importing Role", dashboardParameters);

                            order++;
                            Dashboard dashboard = null;

                            // See if the dashboard exists in the database, based on the rule identifier from the Twitter API.
                            dashboard = await dashboardService.GetByTwitterRuleAsync(ruleEntry.Id);
                            var dashboardCreate = dashboard == null;

                            if (dashboardCreate)
                            {
                                // Create a new dashboard if it doesn't exist in the database.
                                dashboard = new Dashboard();
                            }
                            dashboard.TwitterRuleId = ruleEntry.Id;
                            dashboard.Title = ruleEntry.Tag;
                            dashboard.SearchQuery = ruleEntry.Value;
                            dashboard.Order = order;

                            if (dashboardCreate)
                            {
                                // Create the dashboard to the database.
                                await dashboardService.CreateAsync(dashboard);
                            }
                            else
                            {
                                // Update the dashboard to the database.
                                await dashboardService.UpdateAsync(dashboard.Id, dashboard);
                            }


                            // Acknowledge that the dashboard has been updated.
                            updatedDashboardIds.Add(dashboard.Id);

                            _logger.LogWithParameters(LogLevel.Debug, string.Format("Rule has been imported to Dashboard (id: '{0}').", dashboard.Id), dashboardParameters);
                            _logger.LogWithParameters(LogLevel.Information, "Finish importing rule", dashboardParameters);
                        }
                    }

                    // Remove any missing dashboards & tweets from the database.
                    using (var dbContextTransaction = blashDbContext.Database.BeginTransaction())
                    {
                        await dashboardTweetService.DeleteMissingTwitterRuleLinkAsync(updatedDashboardIds); // Delete any dashboard/tweet relationships in dashboards that haven't been updated.
                        await tweetService.DeleteMissingTweetsFromDashboardAsync(); // Delete any tweets that aren't assigned to a dashboard.
                        await authorService.DeleteMissingTweetsAsync(); // Delete any authors that aren't assigned to any tweets.
                        await dashboardService.DeleteMissingTwitterRuleAsync(updatedDashboardIds); // Delete any dashboards that haven't been updated.

                        await dbContextTransaction.CommitAsync(); // Commit query to the database.
                    }
                }
            }
            _logger.LogWithParameters(LogLevel.Information, "Finish importing rules from Twitter API", parameters);

        }
    }
}
