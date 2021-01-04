using System.Collections.Generic;

namespace RoundTheCode.Blash.Data.Results.CreateTweetObjects
{
    /// <summary>
    /// Stores a list of all the tweets that have been imported from the Twitter API, and created into the Blash database.
    /// </summary>
    public class CreateTweetResult
    {
        /// <summary>
        /// A list of all the tweets and the dashboards that they belong to.
        /// </summary>
        public List<CreateTweetData> Data { get; set; }
    }
}
