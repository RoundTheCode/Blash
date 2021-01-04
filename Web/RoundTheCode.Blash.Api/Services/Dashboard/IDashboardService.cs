using System.Collections.Generic;
using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Data.DashboardObjects;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// A service used for the <see cref="Dashboard"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public interface IDashboardService : IBaseService<Dashboard>
    {
        /// <summary>
        /// Gets an existing record for <see cref="Dashboard"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the record we wish to retrieve.</param>
        /// <returns>The instance of <see cref="Dashboard"/> that was retrieved from <see cref="BlashDbContext"/>.</returns>
        Task<Dashboard> GetAsync(int id);

        /// <summary>
        /// Gets all <see cref="Dashboard"/> records.
        /// </summary>
        /// <returns>Gets a list of all the dashboards.</returns>
        Task<List<Dashboard>> GetAllAsync();

        /// <summary>
        /// Gets an dashboard record from the <see cref="BlashDbContext"/> by passing in the Twitter API Rule ID.
        /// </summary>
        /// <param name="twitterRuleId">The rule identifier from the Twitter API.</param>
        /// <returns>The dashboard record returned from <see cref="BlashDbContext"/>, (or null if it cannot be found).</returns>
        Task<Dashboard> GetByTwitterRuleAsync(string twitterRuleId);

        /// <summary>
        /// Deletes any dashboards that are not included in the dashboard Id's.
        /// </summary>
        /// <param name="updatedDashboardIds">A list of all the dashboards that have been updated.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteMissingTwitterRuleAsync(List<int> updatedDashboardIds);

    }
}
