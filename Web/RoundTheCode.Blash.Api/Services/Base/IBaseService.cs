using System.Threading.Tasks;
using RoundTheCode.Blash.Data.Data.BaseObjects;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// The abstract class used for services. Includes default CRUD methods for a entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity associated with the CRUD methods.</typeparam>
    public interface IBaseService<TEntity>
        where TEntity : class, IBase
    {
        /// <summary>
        /// Creates a new record for <see cref="TEntity"/> in the <see cref="BlashDbContext"/>.
        /// </summary>
        /// <param name="entity">An instance of <see cref="TEntity"/></param>
        /// <returns>The instance of <see cref="TEntity"/> that was created, including the unique identifier.</returns>
        Task<TEntity> CreateAsync(TEntity entity);

        /// <summary>
        /// Updates an existing record for <see cref="TEntity"/> in the <see cref="BlashDbContext"/>.
        /// </summary>
        /// <param name="id">The identifier of the entity that is to be updated.</param>
        /// <param name="updateEntity">An instance of the entity that is to be updated.</param>
        /// <returns>The instance of <see cref="TEntity"/> that was updated.</returns>
        Task<TEntity> UpdateAsync(int id, TEntity updateEntity);

        /// <summary>
        /// Gets an existing record for <see cref="TEntity"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the record we wish to retrieve.</param>
        /// <returns>The instance of <see cref="TEntity"/> that was retrieved from <see cref="BlashDbContext"/>.</returns>
        Task DeleteAsync(int id);
    }
}
