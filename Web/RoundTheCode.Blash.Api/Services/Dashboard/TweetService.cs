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
    /// A service used for the <see cref="Tweet"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public class TweetService : BaseService<Tweet>, ITweetService
    {
        /// <summary>
        /// Creates a new instance of <see cref="TweetService"/>.
        /// </summary>
        /// <param name="blashDbContext">An instance of <see cref="BlashDbContext"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        public TweetService(BlashDbContext blashDbContext, ILogger<TweetService> logger) : base(blashDbContext, logger) { }

        /// <summary>
        /// Get a tweet from the <see cref="BlashDbContext"/>.
        /// </summary>
        /// <param name="twitterTweetId">The tweet identifier from the Twitter API.</param>
        /// <returns>The tweet that corresponds to the tweet identifier from the Twitter API.</returns>
        public async Task<Tweet> GetByTwitterTweetAsync(string twitterTweetId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetByTwitterTweetAsync");
            parameters.Add("Twitter Tweet ID", twitterTweetId);

            try
            {
                // Gets the tweet from the DbContext that is assigned that tweet id.
                return await _blashDbContext.TweetEntities.FirstOrDefaultAsync(tweet => tweet.TwitterTweetId == twitterTweetId);
            }
            catch (Exception exception)
            {
                // Logs an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }

        /// <summary>
        /// Get a list of tweets for a dashboard.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier.</param>
        /// <returns>A list of tweets that are assigned to that dashboard.</returns>
        public async Task<List<Tweet>> GetByDashboardAsync(int dashboardId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetByDashboardAsync");
            parameters.Add("Dashboard ID", dashboardId);

            try
            {
                // Gets all tweets with the author for a particular dashboard.
                return await _blashDbContext.TweetEntities.Include(tweet => tweet.Author).Where(tweet => _blashDbContext.DashboardTweetEntities.Any(dashboardTweet => dashboardTweet.DashboardId == dashboardId && tweet.Id == dashboardTweet.TweetId)).OrderByDescending(tweet => tweet.TwitterPublished).ToListAsync();
            }
            catch (Exception exception)
            {
                // Logs an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }

        /// <summary>
        /// Delete any tweets where the tweet isn't part of a dashboard/tweet relationship.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteMissingTweetsFromDashboardAsync()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteMissingTweetsFromDashboardAsync");

            try
            {
                // Gets all tweets where there is no dashboard/tweet relationship.
                var tweetsToDelete = await _blashDbContext.TweetEntities.Where(tweet => !_blashDbContext.DashboardTweetEntities.Any(dashboardTweet => dashboardTweet.TweetId == tweet.Id)).ToListAsync();

                // Set them as deleted.
                tweetsToDelete.ForEach(tweet =>
                {
                    _blashDbContext.Entry(tweet).State = EntityState.Deleted;
                });
                await _blashDbContext.SaveChangesAsync(); // Save changes to the DbContext.
            }
            catch (Exception exception)
            {
                // Logs an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }
    }
}
