using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using RoundTheCode.Blash.Shared.Logging.Extensions;

namespace RoundTheCode.Blash.Api.Background.Jobs
{
    /// <summary>
    /// A service that runs background jobs.
    /// </summary>
    public class TwitterIntegrationJobService : ITwitterIntegrationJobService
    {
        private SemaphoreSlim JobRunningLock;
        protected readonly ILogger<TwitterIntegrationJobService> _logger;
        protected readonly IHostApplicationLifetime _hostApplicationLifetime;

        /// <summary>
        /// Creates a new instance of <see cref="TwitterIntegrationJobService"/>.
        /// </summary>
        /// <param name="logger">An instance of <see cref="ILogger"/> for writing logs.</param>
        /// <param name="hostApplicationLifetime">Used to get the cancellation token.</param>
        public TwitterIntegrationJobService([NotNull] ILogger<TwitterIntegrationJobService> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;

            JobRunningLock = new SemaphoreSlim(1, 1); // Ensures that only one job is ran at a time.
        }

        /// <summary>
        /// Method to run a job.
        /// </summary>
        /// <param name="twitterIntegrationJob">An instance of the job that will be ran.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task RunJobAsync(TwitterIntegrationJob twitterIntegrationJob)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "RunJobAsync");
            parameters.Add("Job ID", twitterIntegrationJob.Id.ToString());

            // Check there is an instance of the job and that it has not been completed.
            if (twitterIntegrationJob == null || twitterIntegrationJob.JobActionAsync == null || twitterIntegrationJob.Completed.HasValue)
            {
                return;
            }

            try
            {
                // Ensure that only one job can be run at a time.
                _logger.LogWithParameters(LogLevel.Debug, "Wait for all other jobs to complete.", parameters);
                await JobRunningLock.WaitAsync(_hostApplicationLifetime.ApplicationStopping); // Only one job can run at one time.
                _logger.LogWithParameters(LogLevel.Information, "Start running job.", parameters);

                // Invoke the job, passing in the job's identifier (for logging purposes).
                await twitterIntegrationJob.JobActionAsync.Invoke(twitterIntegrationJob.Id);
            }
            catch (Exception exception)
            {
                // Log the error it is failes.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
            }
            finally
            {               
                twitterIntegrationJob.MarkAsComplete(); // Mark the job as complete.
                _logger.LogWithParameters(LogLevel.Information, "Finish running job.", parameters);
                JobRunningLock.Release(); // Release the lock so other jobs can run.
            }
        }
    }
}
