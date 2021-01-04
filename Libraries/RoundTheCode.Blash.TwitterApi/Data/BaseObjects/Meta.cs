using Newtonsoft.Json;

namespace RoundTheCode.Blash.TwitterApi.Data.BaseObjects
{
    /// <summary>
    /// The meta object that gets used when retrieving tweets from the Twitter API.
    /// </summary>
    public class Meta
    {
        /// <summary>
        /// The number of results returned from the Twitter API.
        /// </summary>
        [JsonProperty("result_count")]
        public int ResultCount { get; set; }
    }
}
