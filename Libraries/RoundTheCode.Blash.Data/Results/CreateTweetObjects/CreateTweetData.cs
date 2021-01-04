using System.Collections.Generic;
using RoundTheCode.Blash.Data.Data.DashboardObjects;

namespace RoundTheCode.Blash.Data.Results.CreateTweetObjects
{
    /// <summary>
    /// Stores information about a Tweet and what dashboards the tweet is linked to.
    /// </summary>
    public class CreateTweetData
    {
        /// <summary>
        /// The tweet object.
        /// </summary>
        public Tweet Tweet { get; }

        /// <summary>
        /// A list of all the dashboards that the tweet object belongs to.
        /// </summary>
        public List<DashboardTweet> DashboardTweets { get; }

        /// <summary>
        /// Creates a new instance of CreateTweetData.
        /// </summary>
        /// <param name="tweet">The tweet object.</param>
        /// <param name="dashboardTweets">A list of all the dashboards that the tweet object belongs to.</param>
        public CreateTweetData(Tweet tweet, List<DashboardTweet> dashboardTweets)
        {
            Tweet = tweet;
            DashboardTweets = dashboardTweets;
        }
    }
}
