using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using RoundTheCode.Blash.Data.Data.BaseObjects;

namespace RoundTheCode.Blash.Data.Data.DashboardObjects
{
    /// <summary>
    /// Used to communicate with the blash.Author table in the database.
    /// </summary>
    [Table("Author", Schema = "blash")]
    public class Author : Base
    {
        /// <summary>
        /// The author's ID from the Twitter API.
        /// </summary>
        public string TwitterAuthorId { get; set; }

        /// <summary>
        /// The author's name from the Twitter API.
        /// </summary>
        public string TwitterName { get; set; }

        /// <summary>
        /// The author's handle e.g. @RoundTheCode from the Twitter API.
        /// </summary>
        public string TwitterHandle { get; set; }

        /// <summary>
        /// The author's image profile URL from the Twitter API.
        /// </summary>
        public string TwitterImageUrl { get; set; }

        /// <summary>
        /// Used when creating a model against the Db Context. Ensure's that properties in the database have maximum lengths.
        /// </summary>
        /// <param name="builder">The entity type builder for Entity Framework.</param>
        public static void OnModelCreating(EntityTypeBuilder<Author> builder)
        {
            OnModelCreating<Author>(builder);
            builder.Property(author => author.TwitterAuthorId).HasMaxLength(50);
            builder.Property(author => author.TwitterName).HasMaxLength(200);
            builder.Property(author => author.TwitterHandle).HasMaxLength(200);
            builder.Property(author => author.TwitterImageUrl).HasMaxLength(500);
        }

    }
}
