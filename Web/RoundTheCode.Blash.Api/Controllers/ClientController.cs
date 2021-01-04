using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Extensions;
using RoundTheCode.Blash.Api.Services;
using RoundTheCode.Blash.Shared.Logging.Extensions;

namespace RoundTheCode.Blash.Api.Controllers
{
    /// <summary>
    /// A controller used by the client (Blazor Wasm application).
    /// </summary>
    [Route("api/client")]
    public class ClientController : Controller
    {
        protected readonly ILogger<ClientController> _logger;
        protected readonly IDashboardService _dashboardService;
        protected readonly ITweetService _tweetService;

        /// <summary>
        /// Creates a new instance of <see cref="ClientController"/>.
        /// </summary>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        /// <param name="dashboardService">An instance of <see cref="IDashboardService"/>.</param>
        /// <param name="tweetService">An instance of <see cref="ITweetService"/>.</param>
        public ClientController([NotNull] ILogger<ClientController> logger, [NotNull] IDashboardService dashboardService, [NotNull] ITweetService tweetService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _tweetService = tweetService;
        }

        /// <summary>
        /// Get all dashboards and the tweets that belong to that dashboard from the Blash database.
        /// </summary>
        /// <returns>The response.</returns>
        [HttpGet("dashboard-tweets")]
        public async Task<IActionResult> GetDashboardAndTweetsAsync()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetDashboardAndTweetsAsync");

            try
            {
                // Returns a json with a list of all the dashboards, and their tweets.
                return Json(new { Success = true, Result = await ApiExtensions.GetDashboardAndTweetsAsync(await _dashboardService.GetAllAsync(), _tweetService) }) ;
            }
            catch (Exception exception)
            {
                // Returns a 500 error and logs the exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                return Json(new { error = new { code = Response.StatusCode, message = exception.Message } });
            }
        }
    }
}
