using System.Collections.Generic;
using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Models;
using RoundTheCode.Blash.Data.Results;

namespace RoundTheCode.Blash.BlazorWasm.Services.Dashboard
{
    /// <summary>
    /// A service used with communication with the API when performing dashboard tasks.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Creates a dashboard with the API.
        /// </summary>
        /// <param name="dashboardModel">An instance of <see cref="DashboardModel"/>.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        /// <returns></returns>
        Task CreateDashboardAsync(DashboardModel dashboardModel);

        /// <summary>
        /// Gets a list of all the dashboards and their tweets.
        /// </summary>
        /// <returns>A list of dashboards and their tweets.</returns>
        Task<List<DashboardAndTweetsResult>> GetDashboardAndTweetsAsync();

        /// <summary>
        /// Deletes a dashboard.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteAsync(int dashboardId);
    }
}
