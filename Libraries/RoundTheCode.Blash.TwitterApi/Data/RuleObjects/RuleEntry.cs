using Newtonsoft.Json;

namespace RoundTheCode.Blash.TwitterApi.Data.RuleObjects
{
    /// <summary>
    /// A rule entry assigned to the Twitter API.
    /// </summary>
    public class RuleEntry
    {
        /// <summary>
        /// A unique key to identify the rule entry.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The rule's value which is what the Twitter API uses to search for tweets.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// The rule's custom name to make it easily identifiable.
        /// </summary>
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
}
