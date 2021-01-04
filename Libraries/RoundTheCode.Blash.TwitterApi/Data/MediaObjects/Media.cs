using Newtonsoft.Json;

namespace RoundTheCode.Blash.TwitterApi.Data.MediaObjects
{
    /// <summary>
    /// The media object that stores media information about a tweet from the Twitter API.
    /// </summary>
    public class Media
    {
        /// <summary>
        /// A unique key to identify the media record.
        /// </summary>
        [JsonProperty("media_key")]
        public string MediaKey { get; set; }

        /// <summary>
        /// The media's type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// The media's full URL.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
