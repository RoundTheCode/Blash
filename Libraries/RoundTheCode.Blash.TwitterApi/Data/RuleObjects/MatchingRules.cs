using Newtonsoft.Json;

namespace RoundTheCode.Blash.TwitterApi.Data.RuleObjects
{
    /// <summary>
    /// The rules that a tweet belongs to from the Twitter API.
    /// </summary>
    public class MatchingRules
    {
        /// <summary>
        /// A unique key to identify the matching rule.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The rule's custom name to make it easily identifiable.
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
}
