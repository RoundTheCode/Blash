using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.Data.Results;
using RoundTheCode.Blash.Data.Results.CreateTweetObjects;

namespace RoundTheCode.Blash.BlazorWasm.Services.Api
{
    /// <summary>
    /// A service used with communication with the API.
    /// </summary>
    public class ApiService : IApiService
    {
        /// <summary>
        /// The connection with the SignalR hub.
        /// </summary>
        protected HubConnection HubConnection { get; set; }

        /// <summary>
        /// The API's host address, retreived from appsettings.json.
        /// </summary>
        public string ApiHost { get; init; }

        /// <summary>
        /// When the connection between the SignalR hub is closed.
        /// </summary>
        public event Func<Exception, Task> OnHubClosedAsync;

        /// <summary>
        /// When the connection between the SignalR hub is attempting to reconnect.
        /// </summary>
        public event Func<Exception, Task> OnHubReconnectingAsync;

        /// <summary>
        /// When the connection between the SignalR hub has reconnected.
        /// </summary>
        public event Func<string, Task> OnHubReconnectedAsync;

        /// <summary>
        /// When a tweet is created.
        /// </summary>
        public event Func<CreateTweetResult, Task> OnTweetCreatedAsync;

        /// <summary>
        /// When the recent tweets is synced up.
        /// </summary>
        public event Func<List<DashboardAndTweetsResult>, bool, Task> OnRecentTweetsSyncAsync;

        /// <summary>
        /// When a dashboard is created.
        /// </summary>
        public event Func<List<DashboardAndTweetsResult>, Task> OnDashboardCreatedAsync;

        /// <summary>
        /// When a dashboard is deleted.
        /// </summary>
        public event Func<int, Task> OnDashboardDeletedAsync;

        /// <summary>
        /// When a tweet is deleted from a dashboard.
        /// </summary>
        public event Func<IList<DashboardTweet>, Task> OnDashboardTweetDeletedAsync;

        /// <summary>
        /// Creates a new instance of <see cref="ApiService"/>.
        /// </summary>
        /// <param name="configuration">An instance of <see cref="IConfiguration"/> type.</param>
        public ApiService(IConfiguration configuration)
        {
            ApiHost = configuration.GetSection("ApiHost").Value.ToString(); // Get the API host from the configuration.
            HubConnection = new HubConnectionBuilder().WithUrl(string.Format("{0}/blash-hub", ApiHost)).Build(); // Build the connection with the SignalR hub.
               
            HubConnection.Closed += async (Exception exception) =>
            {
                // Invoke when the hub connection is closed.
                await OnHubClosedAsync(exception);
            };

            HubConnection.Reconnecting += async (Exception exception) =>
            {
                // Invoke when the hub connection is reconnecting.
                await OnHubReconnectingAsync(exception);
            };

            HubConnection.Reconnected += async (string connectionId) =>
            {
                // Invoke when the hub connection is reconnected..
                await OnHubReconnectedAsync(connectionId);
            };

            HubConnection.On("createTweet", async (CreateTweetResult createTweetResult) =>
            {
                // Invoke when a tweet is created.
                await OnTweetCreatedAsync?.Invoke(createTweetResult);
            });

            HubConnection.On("recentTweetsSync", async (List<DashboardAndTweetsResult> dashboardAndTweets, bool refresh) =>
            {
                // Invoke when the recent tweets is synced up.
                await OnRecentTweetsSyncAsync?.Invoke(dashboardAndTweets, refresh);
            });

            HubConnection.On("createDashboard", async (List<DashboardAndTweetsResult> dashboardAndTweets) => 
            {
                // Invoked when a dashboard is created.
                await OnDashboardCreatedAsync?.Invoke(dashboardAndTweets);
            });

            HubConnection.On("deleteDashboard", async (int id) =>
            {
                // Invoked when a dashboard is deleted.
                await OnDashboardDeletedAsync?.Invoke(id);
            });

            HubConnection.On("deleteDashboardTweet", async (IList<DashboardTweet> dashboardTweetsToDelete) =>
            {
                // Invoked when a tweet is deleted from a dashboard.
                await OnDashboardTweetDeletedAsync?.Invoke(dashboardTweetsToDelete);
            });
        }

        /// <summary>
        /// Attempt to start the connection with a SignalR hub.
        /// </summary>
        /// <returns>The current state of the SignalR hub.</returns>
        public async Task<HubConnectionState> AttemptToStartHubConnectionAsync()
        {
            try
            {
                await HubConnection.StartAsync();
            }
            catch (Exception)
            {
            }
            return HubConnection.State;
        }
    }
}
