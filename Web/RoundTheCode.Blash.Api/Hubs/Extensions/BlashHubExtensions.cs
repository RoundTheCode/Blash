using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.Data.Results;
using RoundTheCode.Blash.Data.Results.CreateTweetObjects;

namespace RoundTheCode.Blash.Api.Hubs.Extensions
{
    /// <summary>
    /// A list of extensions for the Blash Chat Hub used in SignalR.
    /// </summary>
    public static class BlashHubExtensions
    {
        /// <summary>
        /// Sends results of the <see cref="RecentTweetsSyncAsync"/> job to the connected clients in the Blash Chat Hub in SignalR.
        /// </summary>
        /// <param name="blashHub">The SignalR chat hub context.</param>
        /// <param name="dashboardAndTweetsResults">A list of all the dashboard and tweets imported.</param>
        /// <param name="refresh">Whether it's a full refresh of the dashboard.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public static async Task RecentTweetsSyncAsync(this IHubContext<BlashHub> blashHub, List<DashboardAndTweetsResult> dashboardAndTweetsResults, bool refresh, CancellationToken cancellationToken)
        {
            await blashHub.Clients.All.SendAsync("recentTweetsSync", dashboardAndTweetsResults, refresh, cancellationToken);
        }

        /// <summary>
        /// Send results when a dashboard is created to the connected clients in the Blash Chat Hub in SignalR.
        /// </summary>
        /// <param name="blashHub">The SignalR chat hub context.</param>
        /// <param name="dashboards">A list of dashboards created.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public static async Task CreateDashboardAsync(this IHubContext<BlashHub> blashHub, List<Dashboard> dashboards, CancellationToken cancellationToken)
        {
            await blashHub.Clients.All.SendAsync("createDashboard", dashboards, cancellationToken);
        }

        /// <summary>
        ///  Send results when a dashboard is deleted to the connected clients in the Blash Chat Hub in SignalR.
        /// </summary>
        /// <param name="blashHub">The SignalR chat hub context.</param>
        /// <param name="id">The dashboard identifier from the Blash database that has been deleted.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public static async Task DeleteDashboardAsync(this IHubContext<BlashHub> blashHub, int id, CancellationToken cancellationToken)
        {
            await blashHub.Clients.All.SendAsync("deleteDashboard", id, cancellationToken);
        }

        /// <summary>
        /// Sends results when a tweet is created to the connected clients in the Blash Chat Hub in SignalR.
        /// </summary>
        /// <param name="blashHub">The SignalR chat hub context.</param>
        /// <param name="createTweetResult">The create tweet result.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public static async Task CreateTweetAsync(this IHubContext<BlashHub> blashHub, CreateTweetResult createTweetResult, CancellationToken cancellationToken)
        {
            await blashHub.Clients.All.SendAsync("createTweet", createTweetResult, cancellationToken);
        }

        /// <summary>
        /// Sends results when a relationship between a dashboard & a tweet is deleted to the connected clients in the Blash Chat Hub in SignalR.
        /// </summary>
        /// <param name="blashHub">The SignalR chat hub context.</param>
        /// <param name="dashboardTweetsToDelete">A list of all the dashboard & tweet relationships that were deleted.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public static async Task DeleteDashboardTweetAsync(this IHubContext<BlashHub> blashHub, IList<DashboardTweet> dashboardTweetsToDelete, CancellationToken cancellationToken)
        {
            await blashHub.Clients.All.SendAsync("deleteDashboardTweet", dashboardTweetsToDelete, cancellationToken);
        }

    }
}
