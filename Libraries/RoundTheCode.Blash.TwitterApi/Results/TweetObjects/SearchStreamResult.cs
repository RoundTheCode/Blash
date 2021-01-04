using Newtonsoft.Json;
using System.Collections.Generic;
using RoundTheCode.Blash.TwitterApi.Data.BaseObjects;
using RoundTheCode.Blash.TwitterApi.Data.RuleObjects;
using RoundTheCode.Blash.TwitterApi.Data.TweetObjects;

namespace RoundTheCode.Blash.TwitterApi.Models
{
    /// <summary>
    /// The stored object when retrieving a Tweet from the Twitter API search stream method.
    /// </summary>
    public class SearchStreamResult
    {
        /// <summary>
        /// The tweet returned.
        /// </summary>
        [JsonProperty("data")]
        public Tweet Tweet { get; set; }

        /// <summary>
        /// A list of user's and media associated with the tweet. 
        /// </summary>
        [JsonProperty("includes")]
        public Includes Includes { get; set; }

        /// <summary>
        /// A list of all the matching rules that the tweet belongs to.
        /// </summary>
        [JsonProperty("matching_rules")]
        public List<MatchingRules> MatchingRules { get; set; }
    }
}
