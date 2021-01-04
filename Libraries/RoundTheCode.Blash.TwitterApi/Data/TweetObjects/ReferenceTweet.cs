using Newtonsoft.Json;

namespace RoundTheCode.Blash.TwitterApi.Data.TweetObjects
{
    /// <summary>
    /// The reference tweet object, which is used to store any tweets that have been retweeted, or quoted.
    /// </summary>
    public class ReferenceTweet
    {
        /// <summary>
        /// What type of tweet is the reference.
        /// </summary>
        [JsonProperty("type")]
        public string ReferenceType { get; set; }

        /// <summary>
        /// A unique identifier of the reference tweet.
        /// </summary>
        [JsonProperty("string")]
        public string Id { get; set; }
    }
}
