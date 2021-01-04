using Newtonsoft.Json;
using System.Collections.Generic;
using RoundTheCode.Blash.Data.Data.DashboardObjects;

namespace RoundTheCode.Blash.Data.Results
{
    /// <summary>
    /// Used when importing tweets to a dashboard, or creating a new dashboard. Gets the dashboard object, and all the tweets that belong to that dashboard.
    /// </summary>
    public class DashboardAndTweetsResult
    {
        /// <summary>
        /// The dashboard object from the Blash database.
        /// </summary>
        [JsonProperty("dashboard")]
        public Dashboard Dashboard { get; }

        /// <summary>
        /// A list of tweet objects from the Blash database that belong to the corresponding dashboard.
        /// </summary>
        [JsonProperty("tweets")]
        public List<Tweet> Tweets { get; }

        /// <summary>
        /// Creates a new instance of <paramref name="DashboardAndTweetsResult"/>.
        /// </summary>
        /// <param name="dashboard">The dashboard object from the Blash database.</param>
        /// <param name="tweets">A list of tweet objects from the Blash database that belong to the corresponding dashboard.</param>
        public DashboardAndTweetsResult(Dashboard dashboard, List<Tweet> tweets)
        {
            Dashboard = dashboard;
            Tweets = tweets;
        }
    }
}

