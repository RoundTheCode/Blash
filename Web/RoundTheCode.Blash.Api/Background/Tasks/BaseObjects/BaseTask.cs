using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using RoundTheCode.Blash.TwitterApi.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using RoundTheCode.Blash.Shared.Logging.Extensions;

namespace RoundTheCode.Blash.Api.Background.Tasks.BaseObjects
{
    /// <summary>
    /// Creates an abstract class for a task.
    /// </summary>
    /// <typeparam name="TTask">The task type that the instance belongs to.</typeparam>
    public abstract class BaseTask<TTask>
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly CancellationToken _cancellationToken;
        protected readonly ILogger<TTask> _logger;

        /// <summary>
        /// Whether the task is running or not.
        /// </summary>
        public bool TaskRunning { get; private set; }

        /// <summary>
        /// The method executed when the task is ran.
        /// </summary>
        /// <param name="id">The job identifier.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        protected abstract Task TaskActionAsync(Guid? id);

        /// <summary>
        /// Creates a new instance of <see cref="BaseTask{TTask}"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used for dependency injection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected BaseTask([NotNull] IServiceProvider serviceProvider, [NotNull] CancellationToken cancellationToken)
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _logger = serviceProvider.GetService<ILogger<TTask>>();
        }

        /// <summary>
        /// Public method used to run the job.
        /// </summary>
        /// <param name="id">The job identifier.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task RunAsync(Guid? id)
        {
            await RunTaskAsync(id);
        }

        /// <summary>
        /// Private method used to run the job. Logs any exceptions.
        /// </summary>
        /// <param name="id">The job identifier.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        private async Task RunTaskAsync(Guid? id)
        {
            TaskRunning = true;
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "RunTaskAsync");

            if (id != null)
            {
                parameters.Add("Job ID", id);
            }

            try
            {
                await TaskActionAsync(id);
            }
            catch (TwitterApiException exception)
            {
                // Log any Twitter exceptions
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
            catch (Exception exception)
            {
                // Log any other exceptions.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
            finally
            {
                // State that the job is no longer running.
                TaskRunning = false;
            }
        }
    }
}