using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.Api.Configuration
{
    /// <summary>
    /// Stores the Tweet settings which are retrieved from the API section in appsettings.json.
    /// </summary>
    public class TweetConfiguration
    {
        /// <summary>
        /// The interval time as to sync all tweets from the Twitter API.
        /// </summary>
        public TimeSpan? SyncRecentSchedule { get; init; }

        /// <summary>
        /// The maximum number of tweets per dashboard.
        /// </summary>
        public int? MaxPerDashboard { get; init; }
    }
}
