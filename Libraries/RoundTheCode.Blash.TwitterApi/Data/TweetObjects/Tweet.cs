using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RoundTheCode.Blash.TwitterApi.Data.AttachmentObjects;

namespace RoundTheCode.Blash.TwitterApi.Data.TweetObjects
{
    /// <summary>
    /// The tweet object that gets used when retrieving a tweet from the Twitter API.
    /// </summary>
    public class Tweet
    {
        /// <summary>
        /// A unique key to identify the tweet.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The tweet's content.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Any attachments assigned to the tweet.
        /// </summary>
        [JsonProperty("attachments")]
        public Attachment Attachments { get; set; }

        /// <summary>
        /// The tweet's author identifier.
        /// </summary>
        [JsonProperty("author_id")]
        public string AuthorId { get; set; }

        /// <summary>
        /// The tweet's created date and time.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Any tweets that are referenced, such as a quote or a retweet.
        /// </summary>
        [JsonProperty("referenced_tweets")]
        public List<ReferenceTweet> ReferencedTweets { get; set; }
    }
}
