using System.Collections.Generic;
using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.Api.Data;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// A service used for the <see cref="Tweet"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public interface ITweetService : IBaseService<Tweet>
    {
        /// <summary>
        /// Get a tweet from the <see cref="BlashDbContext"/>
        /// </summary>
        /// <param name="twitterTweetId">The tweet identifier from the Twitter API</param>
        /// <returns>The tweet that corresponds to the tweet identifier from the Twitter API.</returns>
        Task<Tweet> GetByTwitterTweetAsync(string twitterTweetId);

        /// <summary>
        /// Get a list of tweets for a dashboard.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier.</param>
        /// <returns>A list of tweets that are assigned to that dashboard.</returns>
        Task<List<Tweet>> GetByDashboardAsync(int dashboardId);

        /// <summary>
        /// Delete any tweets where the tweet isn't part of a dashboard/tweet relationship.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteMissingTweetsFromDashboardAsync();
    }
}
