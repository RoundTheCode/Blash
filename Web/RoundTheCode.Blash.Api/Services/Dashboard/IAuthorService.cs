using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Data.DashboardObjects;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// A service used for the <see cref="Author"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public interface IAuthorService : IBaseService<Author>
    {
        /// <summary>
        /// Gets an author record from the <see cref="BlashDbContext"/> by passing in the Twitter API Author ID.
        /// </summary>
        /// <param name="twitterAuthorId">The author identifier from the Twitter API.</param>
        /// <returns>The author record returned from <see cref="BlashDbContext"/>, (or null if it cannot be found).</returns>
        /// <returns></returns>
        Task<Author> GetByTwitterAuthorAsync(string twitterAuthorId);

        /// <summary>
        /// Delete any author records from <see cref="BlashDbContext"/> where there are no tweets associated with it.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteMissingTweetsAsync();
    }
}
