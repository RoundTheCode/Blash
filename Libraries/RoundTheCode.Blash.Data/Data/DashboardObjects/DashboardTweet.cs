using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using RoundTheCode.Blash.Data.Data.BaseObjects;

namespace RoundTheCode.Blash.Data.Data.DashboardObjects
{
    /// <summary>
    /// Used to communicate with the blash.DashboardTweet table in the database.
    /// </summary>
    [Table("DashboardTweet", Schema = "blash")]
    public class DashboardTweet : Base
    {
        /// <summary>
        /// The Dashboard Id from the database.
        /// </summary>
        public int DashboardId { get; set; }

        /// <summary>
        /// The Tweet Id from the database.
        /// </summary>
        public int TweetId { get; set; }

        /// <summary>
        /// The Dashboard object from the database.
        /// </summary>
        public Dashboard Dashboard { get; set; }

        /// <summary>
        /// The Tweet object from the database.
        /// </summary>
        public Tweet Tweet { get; set; }

        /// <summary>
        /// Used when creating a model against the Db Context.
        /// </summary>
        /// <param name="builder">The entity type builder for Entity Framework.</param>
        public static void OnModelCreating(EntityTypeBuilder<DashboardTweet> builder)
        {
            OnModelCreating<DashboardTweet>(builder);
        }
    }
}
