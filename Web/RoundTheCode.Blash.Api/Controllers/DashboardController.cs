using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Exceptions;
using RoundTheCode.Blash.Api.Hubs;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.Api.Services;
using RoundTheCode.Blash.Shared.Logging.Extensions;
using RoundTheCode.Blash.Data.Models;
using RoundTheCode.Blash.TwitterApi.Data.RuleObjects;
using RoundTheCode.Blash.TwitterApi.Services.RuleObjects;
using RoundTheCode.Blash.Api.Hubs.Extensions;
using RoundTheCode.Blash.Api.Background.Jobs;
using RoundTheCode.Blash.Api.Background.Tasks;
using Microsoft.Extensions.Hosting;
using RoundTheCode.Blash.Api.Data;

namespace RoundTheCode.Blash.Api.Controllers
{
    /// <summary>
    /// A controller used to create and delete dashboards from the API.
    /// </summary>
    [Route("api/dashboard")]
    public class DashboardController : Controller
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IDashboardService _dashboardService;
        protected readonly ITweetService _tweetService;
        protected readonly BlashDbContext _blashDbContext;

        protected readonly ITwitterApiRuleService _twitterApiRuleService;
        protected readonly ILogger<DashboardController> _logger;
        protected readonly ITwitterIntegrationJobService _twitterIntegrationJobService;
        protected readonly IHubContext<BlashHub> _blashHub;

        protected readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// Creates a new instance of <see cref="DashboardController"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="dashboardService">An instance of <see cref="IDashboardService"/>.</param>
        /// <param name="tweetService">An instance of <see cref="ITweetService"/>.</param>
        /// <param name="blashDbContext">An instance of <see cref="BlashDbContext"/>.</param>
        /// <param name="twitterApiRuleService">An instance of <see cref="ITwitterApiRuleService"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        /// <param name="twitterIntegrationJobService">An instance of <see cref="ITwitterIntegrationJobService"/>.</param>
        /// <param name="blashHub">The SignalR chat hub.</param>
        /// <param name="hostApplicationLifetime">An instance of the lifetime, so we can get the cancellation token.</param>
        public DashboardController([NotNull] IServiceProvider serviceProvider, [NotNull] IDashboardService dashboardService, 
            [NotNull] ITweetService tweetService, [NotNull] BlashDbContext blashDbContext,
            [NotNull] ITwitterApiRuleService twitterApiRuleService, [NotNull] ILogger<DashboardController> logger, 
            [NotNull] ITwitterIntegrationJobService twitterIntegrationJobService, [NotNull] IHubContext<BlashHub> blashHub,
            [NotNull] IHostApplicationLifetime hostApplicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _dashboardService = dashboardService;
            _tweetService = tweetService;
            _blashDbContext = blashDbContext;

            _twitterApiRuleService = twitterApiRuleService;
            _logger = logger;
            _twitterIntegrationJobService = twitterIntegrationJobService;
            _blashHub = blashHub;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        /// <summary>
        /// Creates a dashboard to the database.
        /// </summary>
        /// <param name="dashboard">An instance of <see cref="DashboardModel"/>.</param>
        /// <returns>A JSON response of what has been created.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] DashboardModel dashboard)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "CreateAsync");

            try
            {
                if (!ModelState.IsValid)
                {
                    // Throw an error if not valid.
                    throw new Exception("There is an error creating the dashboard.");
                }

                // Create the rule in the Twitter API.
                var ruleResult = await _twitterApiRuleService.CreateRuleAsync(new List<RuleEntry> { new RuleEntry { Tag = dashboard.Title, Value = string.Format("{0} {1}", dashboard.Title, "-is:reply -is:retweet -is:quote") } });

                List<Dashboard> dashboards = null;
                foreach (var ruleEntry in ruleResult.RuleEntries)
                {
                    // Convert the rules into dashboards.
                    dashboards = dashboards ?? new List<Dashboard>();

                    var dashboardRecord = new Dashboard();
                    dashboardRecord.Title = ruleEntry.Tag;
                    dashboardRecord.SearchQuery = ruleEntry.Value;
                    dashboardRecord.TwitterRuleId = ruleEntry.Id;
                    dashboardRecord = await _dashboardService.CreateAsync(dashboardRecord);

                    dashboards.Add(dashboardRecord);

                    // Import the tweets into the dashboard.
                    await ImportTweetsForDashboard(dashboardRecord);
                }

                // Send the fact that a dashboard has been created to the clients connected through SignalR.
                await _blashHub.CreateDashboardAsync(dashboards, _hostApplicationLifetime.ApplicationStopping);

                // Return the created dashboards.
                return Json(new { Success = true, Result = dashboards });
            }
            catch (Exception exception)
            {
                // Returns a 500 error and logs the exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return Json(new { error = new { code = Response.StatusCode, message = exception.Message } });
            }
        }

        /// <summary>
        /// Deletes the dashboard from the database.
        /// </summary>
        /// <param name="id">The dashboard identifier from the Blash database.</param>
        /// <returns>Whether the deletion was successful or not.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteAsync");

            try
            {
                // Does the dashboard exist?
                var dashboard = await _dashboardService.GetAsync(id);

                if (dashboard == null)
                {
                    throw new ApiException(string.Format("Unable to find a dashboard record with ID of '{0}'.", id));
                }

                // Delete the corresponding rule from the Twitter API, by using the TwitterRuleId property in the dashboard.
                await _twitterApiRuleService.DeleteRuleAsync(new List<string> { dashboard.TwitterRuleId });

                // Delete the dashboard from the database.
                await _dashboardService.DeleteAsync(dashboard.Id);



                // Send the fact that a dashboard has been deleted to the clients connected through SignalR.
                await _blashHub.DeleteDashboardAsync(id, _hostApplicationLifetime.ApplicationStopping);

                return Json(new { Success = true });
            }
            catch (Exception exception)
            {
                // Returns a 500 error and logs the exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return Json(new { error = new { code = Response.StatusCode, message = exception.Message } });
            }
        }

        /// <summary>
        /// Runs the recent tweets task for a particular dashboard.
        /// </summary>
        /// <param name="dashboard"></param>
        /// <returns></returns>
        protected async Task ImportTweetsForDashboard(Dashboard dashboard)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "UpdateTweetsForDashboard");
            parameters.Add("DB Dashboard ID", dashboard.Id);

            try
            {
                _logger.LogWithParameters(LogLevel.Information, "Start importing tweets for nearly created dashboard.", parameters);
                var importRecentTweetsFromTwitterApiTask = new ImportRecentTweetsFromTwitterApiTask(_serviceProvider, new System.Threading.CancellationTokenSource().Token, dashboard); // Creates the task.
                await _twitterIntegrationJobService.RunJobAsync(new TwitterIntegrationJob((Guid id) => importRecentTweetsFromTwitterApiTask.RunAsync(id))); // Runs the job.
                _logger.LogWithParameters(LogLevel.Information, "Finish importing tweets for nearly created dashboard.", parameters);
           
            }
            catch (Exception exception)
            {
                // Log any exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }
    }
}
