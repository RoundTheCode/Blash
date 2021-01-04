using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace RoundTheCode.Blash.Data.Data.BaseObjects
{
    /// <summary>
    /// The abstract class that is used for all database objects.
    /// </summary>
    public abstract class Base : IBase
    {
        /// <summary>
        /// The Id of the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The date for when the entity was created.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The date for when the entity was updated.
        /// </summary>
        public DateTimeOffset? Updated { get; set; }

        /// <summary>
        /// Used when creating a model against the Db Context.
        /// </summary>
        /// <typeparam name="TEntity">The database entity.</typeparam>
        /// <param name="builder">The entity type builder for Entity Framework.</param>
        public static void OnModelCreating<TEntity>(EntityTypeBuilder<TEntity> builder)
            where TEntity : class, IBase
        {
            // Ensure that the Id property is the primary key.
            builder.HasKey(entity => entity.Id);
        }
    }
}
