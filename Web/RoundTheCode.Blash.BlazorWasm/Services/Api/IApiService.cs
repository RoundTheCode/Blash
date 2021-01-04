using Microsoft.AspNetCore.SignalR.Client;
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
    public interface IApiService
    {
        /// <summary>
        /// The connection with the SignalR hub.
        /// </summary>
        string ApiHost { get; }

        /// <summary>
        /// When the connection between the SignalR hub is closed.
        /// </summary>
        event Func<Exception, Task> OnHubClosedAsync;

        /// <summary>
        /// When the connection between the SignalR hub is attempting to reconnect.
        /// </summary>
        event Func<Exception, Task> OnHubReconnectingAsync;

        /// <summary>
        /// When the connection between the SignalR hub has reconnected.
        /// </summary>
        event Func<string, Task> OnHubReconnectedAsync;

        /// <summary>
        /// When a tweet is created.
        /// </summary>
        event Func<CreateTweetResult, Task> OnTweetCreatedAsync;

        /// <summary>
        /// When the recent tweets is synced up.
        /// </summary>
        event Func<List<DashboardAndTweetsResult>, bool, Task> OnRecentTweetsSyncAsync;

        /// <summary>
        /// When a dashboard is created.
        /// </summary>
        event Func<List<DashboardAndTweetsResult>, Task> OnDashboardCreatedAsync;

        /// <summary>
        /// When a dashboard is deleted.
        /// </summary>
        event Func<int, Task> OnDashboardDeletedAsync;

        /// <summary>
        /// When a tweet is deleted from a dashboard.
        /// </summary>
        event Func<IList<DashboardTweet>, Task> OnDashboardTweetDeletedAsync;

        /// <summary>
        /// Attempt to start the connection with a SignalR hub.
        /// </summary>
        /// <returns>The current state of the SignalR hub.</returns>
        Task<HubConnectionState> AttemptToStartHubConnectionAsync();
    }
}
