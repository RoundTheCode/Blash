using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Data;
using RoundTheCode.Blash.Data.Data.BaseObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RoundTheCode.Blash.Shared.Logging.Extensions;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// The abstract class used for services. Includes default CRUD methods for a entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity associated with the CRUD methods.</typeparam>
    public abstract class BaseService<TEntity> : IBaseService<TEntity>
        where TEntity : class, IBase
    {
        protected readonly BlashDbContext _blashDbContext;
        protected readonly ILogger<BaseService<TEntity>> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="BaseService{TEntity}"/>.
        /// </summary>
        /// <param name="blashDbContext">An instance of <see cref="BlashDbContext"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        protected BaseService([NotNull] BlashDbContext blashDbContext, [NotNull] ILogger<BaseService<TEntity>> logger)
        {
            _blashDbContext = blashDbContext;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new record for <see cref="TEntity"/> in the <see cref="BlashDbContext"/>.
        /// </summary>
        /// <param name="entity">An instance of <see cref="TEntity"/></param>
        /// <returns>The instance of <see cref="TEntity"/> that was created, including the unique identifier.</returns>
        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", string.Format("CreateAsync<{0}>", typeof(TEntity).Name));

            try {

                entity.Created = DateTimeOffset.Now; // Set the created date/time.
                await _blashDbContext.AddAsync(entity); // Add the entity to the DbContext.
                await _blashDbContext.SaveChangesAsync(); // Save changes.

                return entity; // Returns the entity (including the ID).
            }
            catch (Exception exception)
            {
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing record for <see cref="TEntity"/> in the <see cref="BlashDbContext"/>.
        /// </summary>
        /// <param name="id">The identifier of the entity that is to be updated.</param>
        /// <param name="updateEntity">An instance of the entity that is to be updated.</param>
        /// <returns>The instance of <see cref="TEntity"/> that was updated.</returns>
        public async Task<TEntity> UpdateAsync(int id, TEntity updateEntity)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", string.Format("UpdateAsync<{0}>", typeof(TEntity).Name));
            parameters.Add("Id", id);

            try
            {
                var entity = await GetAsync(id); // Make sure the record exists in the DbContext.

                if (entity == null)
                {
                    return null; // Returns if the entity doesn't exist.
                }

                // Update changes if any of the properties have been modified.
                _blashDbContext.Entry(entity).CurrentValues.SetValues(updateEntity); // Update the curren  entity with the new entity.
                _blashDbContext.Entry(entity).State = EntityState.Modified; // Set the state to being modified.

                if (_blashDbContext.Entry(entity).Properties.Any(property => property.IsModified))
                {
                    // If any properties have been updated, set the updated date, and the save the changes against the DbContext.
                    entity.Updated = DateTimeOffset.Now;
                    await _blashDbContext.SaveChangesAsync();
                }

                return entity; // Returns the entity.
            }
            catch (Exception exception)
            {
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Gets an existing record for <see cref="TEntity"/>. Protected as it's not always needed for each entity.
        /// </summary>
        /// <param name="id">The unique identifier of the record we wish to retrieve.</param>
        /// <returns>The instance of <see cref="TEntity"/> that was retrieved from <see cref="BlashDbContext"/>.</returns>
        protected async Task<TEntity> GetAsync(int id)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", string.Format("GetAsync<{0}>", typeof(TEntity).Name));
            parameters.Add("Id", id);

            try
            {
                return await _blashDbContext.Set<TEntity>().FirstOrDefaultAsync(entity => entity.Id == id); // Gets the record from the DbContext.
            }
            catch (Exception exception)
            {
                // Logs exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Deletes an existing record for <see cref="TEntity"/>.
        /// </summary>
        /// <param name="id">The identifier of the entity that is to be deleted.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public virtual async Task DeleteAsync(int id)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", string.Format("DeleteAsync<{0}>", typeof(TEntity).Name));
            parameters.Add("Id", id);

            try
            {
                var entity = await GetAsync(id); // Make sure the record exists in the DbContext.

                if (entity == null)
                {
                    return;
                }

                _blashDbContext.Entry(entity).State = EntityState.Deleted; // Mark the entity as deleted.
                await _blashDbContext.SaveChangesAsync(); // Save the changes to the DbContext.
            }
            catch (Exception exception)
            {
                // Logs exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }
    }
}
