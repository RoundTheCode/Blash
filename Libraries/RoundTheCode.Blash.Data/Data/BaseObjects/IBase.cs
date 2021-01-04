using System;

namespace RoundTheCode.Blash.Data.Data.BaseObjects
{
    /// <summary>
    /// The interface class that is used for all database objects.
    /// </summary>
    public interface IBase
    {
        /// <summary>
        /// The Id of the entity.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// The date for when the entity was created.
        /// </summary>
        DateTimeOffset Created { get; set; }

        /// <summary>
        /// The date for when the entity was updated.
        /// </summary>
        DateTimeOffset? Updated { get; set; }
    }
}
