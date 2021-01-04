using System.Collections.Generic;
using RoundTheCode.Blash.Data.Data.DashboardObjects;

namespace RoundTheCode.Blash.BlazorWasm.Models
{
    /// <summary>
    /// A model used to display a dashboard and it's tweets.
    /// </summary>
    public class DashboardComponentModel
    {        
        /// <summary>
        /// The instance of the dashboard.
        /// </summary>
        public Dashboard Dashboard { get; set; }

        /// <summary>
        /// A list of tweets associated with the dashboard.
        /// </summary>
        public List<Tweet> Tweets { get; set; }
    }
}
