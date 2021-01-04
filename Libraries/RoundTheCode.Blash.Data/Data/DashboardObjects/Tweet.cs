using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using RoundTheCode.Blash.Data.Data.BaseObjects;

namespace RoundTheCode.Blash.Data.Data.DashboardObjects
{
    /// <summary>
    /// Used to communicate with the blash.Tweet table in the database.
    /// </summary>
    [Table("Tweet", Schema = "blash")]
    public class Tweet : Base
    {
        /// <summary>
        /// The Author Id from the database that the Tweet links to.
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// The tweet ID from the Twitter API.
        /// </summary>
        public string TwitterTweetId { get; set; }

        /// <summary>
        /// The tweet's content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// When the tweet was published. Information retrieved from the Twitter API.
        /// </summary>
        public DateTimeOffset TwitterPublished { get; set; }

        /// <summary>
        /// The Author object from the database.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Used when creating a model against the Db Context. Ensure's that properties in the database have maximum lengths.
        /// </summary>
        /// <param name="builder">The entity type builder for Entity Framework.</param>
        public static void OnModelCreating(EntityTypeBuilder<Tweet> builder)
        {
            OnModelCreating<Tweet>(builder);
            builder.Property(tweet => tweet.TwitterTweetId).HasMaxLength(50);
            builder.Property(tweet => tweet.Content).HasMaxLength(1000);
        }
    }
}
