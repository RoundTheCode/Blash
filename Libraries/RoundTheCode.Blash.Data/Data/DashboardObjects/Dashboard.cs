using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using RoundTheCode.Blash.Data.Data.BaseObjects;


namespace RoundTheCode.Blash.Data.Data.DashboardObjects
{
    /// <summary>
    /// Used to communicate with the blash.Dashboard table in the database.
    /// </summary>
    [Table("Dashboard", Schema = "blash")]
    public class Dashboard : Base
    {
        /// <summary>
        /// The Rule ID from the Twitter API.
        /// </summary>
        public string TwitterRuleId { get; set; }

        /// <summary>
        /// The title of the dashboard.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The query used in the Twitter API to get the tweets.
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// The dashboard's order. The lower the number, the higher it's up the list.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Used when creating a model against the Db Context. Ensure's that properties in the database have maximum lengths.
        /// </summary>
        /// <param name="builder">The entity type builder for Entity Framework.</param>
        public static void OnModelCreating(EntityTypeBuilder<Dashboard> builder)
        {
            OnModelCreating<Dashboard>(builder);
            builder.Property(dashboard => dashboard.Title).HasMaxLength(200);
            builder.Property(dashboard => dashboard.TwitterRuleId).HasMaxLength(50);
            builder.Property(dashboard => dashboard.SearchQuery).HasMaxLength(200);
        }
    }
}
