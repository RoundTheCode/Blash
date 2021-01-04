using System.Collections.Generic;
using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Data.DashboardObjects;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// A service used for the <see cref="DashboardTweet"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public interface IDashboardTweetService : IBaseService<DashboardTweet>
    {
        /// <summary>
        /// Gets the dashboard/tweet record based on the dashboard ID and Tweet ID.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier from <see cref="BlashDbContext"/>.</param>
        /// <param name="tweetId">The tweet identifier from <see cref="BlashDbContext"/>.</param>
        /// <returns>The dashboard tweet relationship returned from <see cref="BlashDbContext"/>.</returns>
        Task<DashboardTweet> GetByDashboardTweetAsync(int dashboardId, int tweetId);

        /// <summary>
        /// Gets the dashboard/tweet record based on the dashboard ID and after a certain record position. Used to restrict the number of tweets per dashboard.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier from <see cref="BlashDbContext"/>.</param>
        /// <param name="afterPosition">The minimum record position to retrieve the records.</param>
        /// <returns>A list of all the dashboard/tweet relationships from <see cref="BlashDbContext"/>.</returns>
        Task<IList<DashboardTweet>> GetByDashboardAndAfterPositionAsync(int dashboardId, int afterPosition);

        /// <summary>
        /// Delete multiple dashboard, tweet relationships.
        /// </summary>
        /// <param name="ids">A list of identifiers to delete.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteMultipleAsync(IList<int> Ids);

        /// <summary>
        /// Deletes any dashboard/tweets relationship, where the tweet wasn't updated.
        /// </summary>
        /// <param name="updatedTweetIds">A list of tweet identifiers that were updated.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteNonUpdatedTweetsAsync(List<int> updatedTweetIds);

        /// <summary>
        /// Deletes any dashboard/tweet relationships where the dashboard hasn't been updated.
        /// </summary>
        /// <param name="updatedDashboardIds"></param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteMissingTwitterRuleLinkAsync(List<int> updatedDashboardIds);
    }
}
