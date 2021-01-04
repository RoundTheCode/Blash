using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Data;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.Shared.Logging.Extensions;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// A service used for the <see cref="DashboardTweet"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public class DashboardTweetService : BaseService<DashboardTweet>, IDashboardTweetService
    {
        /// <summary>
        /// Creates a new instance of <see cref="DashboardTweetService"/>.
        /// </summary>
        /// <param name="blashDbContext">An instance of <see cref="BlashDbContext"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        public DashboardTweetService(BlashDbContext blashDbContext, ILogger<DashboardTweetService> logger) : base(blashDbContext, logger) { }

        /// <summary>
        /// Gets the dashboard/tweet record based on the dashboard ID and Tweet ID.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier from <see cref="BlashDbContext"/>.</param>
        /// <param name="tweetId">The tweet identifier from <see cref="BlashDbContext"/>.</param>
        /// <returns>The dashboard tweet relationship returned from <see cref="BlashDbContext"/>.</returns>
        public async Task<DashboardTweet> GetByDashboardTweetAsync(int dashboardId, int tweetId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetByDashboardTweetAsync");
            parameters.Add("DB Dashboard ID", dashboardId);
            parameters.Add("DB Tweet ID", tweetId);

            try
            {
                // Gets the dashboard/tweet record from the DbContext.
                return await _blashDbContext.DashboardTweetEntities.FirstOrDefaultAsync(dashboardTweet => dashboardTweet.DashboardId == dashboardId && dashboardTweet.TweetId == tweetId);
            }
            catch (Exception exception)
            {
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }

        /// <summary>
        /// Gets the dashboard/tweet record based on the dashboard ID and after a certain record position. Used to restrict the number of tweets per dashboard.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier from <see cref="BlashDbContext"/>.</param>
        /// <param name="afterPosition">The minimum record position to retrieve the records.</param>
        /// <returns>A list of all the dashboard/tweet relationships from <see cref="BlashDbContext"/>.</returns>
        public async Task<IList<DashboardTweet>> GetByDashboardAndAfterPositionAsync(int dashboardId, int afterPosition)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetByDashboardAndAfterPosition");
            parameters.Add("DB Dashboard ID", dashboardId);
            parameters.Add("After Position", afterPosition);

            try
            {
                // Gets all dashboard/tweet entities, where the tweet's record position is after a set position.
                return await _blashDbContext.DashboardTweetEntities.Include(dashboardTweet => dashboardTweet.Tweet)
                    .Where(dashboardTweet => dashboardTweet.DashboardId == dashboardId)
                    .OrderByDescending(dashboardTweet => dashboardTweet.Tweet.TwitterPublished)
                    .Skip(afterPosition)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                // Throws an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }

        /// <summary>
        /// Delete multiple dashboard, tweet relationships.
        /// </summary>
        /// <param name="ids">A list of identifiers to delete.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteMultipleAsync(IList<int> ids)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteMultiple");

            try
            {
                // Get all the dashboard/tweet relationships to delete.
                var deleteDashboardTweets = await _blashDbContext.DashboardTweetEntities.Where(dashboardTweet => ids.Any(dashboardTweetIdToDelete => dashboardTweet.Id == dashboardTweetIdToDelete)).ToListAsync();

                // Mark them as deleted.
                deleteDashboardTweets.ForEach(dashboardTweet =>
                {
                    _blashDbContext.Entry(dashboardTweet).State = EntityState.Deleted;
                });

                // Save change sagainst the DbContext.
                await _blashDbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                // Throws an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }

        /// <summary>
        /// Deletes any dashboard/tweets relationship, where the tweet wasn't updated.
        /// </summary>
        /// <param name="updatedTweetIds">A list of tweet identifiers that were updated.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteNonUpdatedTweetsAsync(List<int> updatedTweetIds)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteNonUpdatedTweetsAsync");

            try { 
                // Get all dashboard/tweet relationships where tweets haven't been updated.
                var deleteDashboardTweets = await _blashDbContext.DashboardTweetEntities.Where(dashboardTweet => !updatedTweetIds.Any(updatedTweetId => dashboardTweet.TweetId == updatedTweetId)).ToListAsync();

                // Remove these relationships.
                deleteDashboardTweets.ForEach(dashboardTweet =>
                {
                    _blashDbContext.Entry(dashboardTweet).State = EntityState.Deleted;
                });

                // Save the changes.
                await _blashDbContext.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                // Throw an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }

        /// <summary>
        /// Deletes any dashboard/tweet relationships where the dashboard hasn't been updated.
        /// </summary>
        /// <param name="updatedDashboardIds"></param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteMissingTwitterRuleLinkAsync(List<int> updatedDashboardIds)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteMissingTwitterRuleLinkAsync");

            try
            {
                // Get all dashboard/tweet relationships where dashboards haven't been updated.
                var dashboardTweetsToDelete = await _blashDbContext.DashboardTweetEntities.Where(dashboardTweet => !updatedDashboardIds.Any(updatedId => updatedId == dashboardTweet.DashboardId)).ToListAsync();

                // Delete the dashboard/tweet relationships where the dashboard hasn't been updated.
                dashboardTweetsToDelete.ForEach(dashboard =>
                {
                    _blashDbContext.Entry(dashboard).State = EntityState.Deleted;
                });
                await _blashDbContext.SaveChangesAsync(); // Save the changes.
            }
            catch (Exception exception)
            {
                // Logs an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        } 
    }
}
