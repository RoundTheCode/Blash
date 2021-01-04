using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Data;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using RoundTheCode.Blash.Shared.Logging.Extensions;

namespace RoundTheCode.Blash.Api.Services
{
    /// <summary>
    /// A service used for the <see cref="Dashboard"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public class DashboardService : BaseService<Dashboard>, IDashboardService
    {
        /// <summary>
        /// Creates a new instance of <see cref="DashboardService"/>.
        /// </summary>
        /// <param name="blashDbContext">An instance of <see cref="BlashDbContext"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        public DashboardService(BlashDbContext blashDbContext, ILogger<DashboardService> logger) : base(blashDbContext, logger) { }

        /// <summary>
        /// Creates a new record for <see cref="Dashboard"/> in the <see cref="BlashDbContext"/>.
        /// </summary>
        /// <param name="entity">An instance of <see cref="Dashboard"/>.</param>
        /// <returns>The instance of <see cref="Dashboard"/> that was created, including the unique identifier.</returns>
        public override async Task<Dashboard> CreateAsync(Dashboard entity)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "CreateAsync");

            try
            {
                var current = _blashDbContext.Database.CurrentTransaction != null; // Can't have two transactions at the same time.

                // Use the current tranasction. If it's null, create a new one.
                using (var dbContextTransaction = _blashDbContext.Database.CurrentTransaction ?? await _blashDbContext.Database.BeginTransactionAsync()) 
                {
                    var order = (_blashDbContext.DashboardEntities.Max(dashboard => (int?)dashboard.Order) ?? 0) + 1;

                    entity.Order = order; // Create a new order.
                    entity = await base.CreateAsync(entity); // Create the entity.

                    if (!current) // If we have created a new transaction in this method, commit it. 
                    {
                        await dbContextTransaction.CommitAsync();
                    }
                }

                return entity; // Return the created entity.
            }
            catch (Exception exception)
            {
                // Throws an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Gets an existing record for <see cref="Dashboard"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the record we wish to retrieve.</param>
        /// <returns>The instance of <see cref="Dashboard"/> that was retrieved from <see cref="BlashDbContext"/>.</returns>
        public new async Task<Dashboard> GetAsync(int id)
        {
            return await base.GetAsync(id);
        }

        /// <summary>
        /// Gets all <see cref="Dashboard"/> records.
        /// </summary>
        /// <returns>Gets a list of all the dashboards.</returns>
        public async Task<List<Dashboard>> GetAllAsync()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetAllAsync");

            try
            {
                // Gets all dashboards, order by the 'Order' property.
                return await _blashDbContext.DashboardEntities.OrderBy(dashboard => dashboard.Order).ToListAsync();
            }
            catch (Exception exception)
            {
                // Throws an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Gets an dashboard record from the <see cref="BlashDbContext"/> by passing in the Twitter API Rule ID.
        /// </summary>
        /// <param name="twitterRuleId">The rule identifier from the Twitter API.</param>
        /// <returns>The dashboard record returned from <see cref="BlashDbContext"/>, (or null if it cannot be found).</returns>
        public async Task<Dashboard> GetByTwitterRuleAsync(string twitterRuleId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetByTwitterRuleAsync");
            parameters.Add("Twitter Rule Id", twitterRuleId);

            try
            {
                // Gets the dashboard entity, based on the Twitter Rule ID.
                return await _blashDbContext.DashboardEntities.FirstOrDefaultAsync(dashboard => dashboard.TwitterRuleId == twitterRuleId);
            }
            catch (Exception exception)
            {
                // Throws an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Gets all dashboard records after a certain order number. Used for re-ordering when a dashboard is deleted.
        /// </summary>
        /// <param name="order">Gets all dashboard records that exceed this order number.</param>
        /// <returns>A list of dashboards.</returns>
        protected async Task<List<Dashboard>> GetAfterOrder(int order)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetAfterOrder");
            parameters.Add("Order", order);

            try
            {
                // Gets all the dashboards that exceed the order number.
                return await _blashDbContext.DashboardEntities.Where(dashboard => dashboard.Order > order).ToListAsync();
            }
            catch (Exception exception)
            {
                // Logs an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Deletes an existing record for <see cref="Dashboard"/>.
        /// </summary>
        /// <param name="id">The identifier of the entity that is to be deleted.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public override async Task DeleteAsync(int id)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteAsync");

            try
            {
                var current = _blashDbContext.Database.CurrentTransaction != null; // Can't have two transactions at the same time.

                // Use the current tranasction. If it's null, create a new one.
                using (var dbContextTransaction =  _blashDbContext.Database.CurrentTransaction ?? await _blashDbContext.Database.BeginTransactionAsync())
                {
                    var entity = await GetAsync(id); // Gets the current entity.

                    if (entity != null)
                    {
                        var order = entity.Order; // Make a note of the order.
                         
                        await base.DeleteAsync(id); // Delete the entity.

                        var dashboardsToUpdate = await GetAfterOrder(order); // Get the dashboards to update.
                        if (dashboardsToUpdate != null && dashboardsToUpdate.Count > 0)
                        {
                            // Decrease each order number by 1.
                            dashboardsToUpdate.ForEach(dashboard =>
                            {
                                dashboard.Order -= 1;
                                _blashDbContext.Entry(dashboard).State = EntityState.Modified; // Mark as modified.
                            });
                            await _blashDbContext.SaveChangesAsync(); // Save the changes.
                        }
                    }

                    if (!current) // If we have created a new transaction in this method, commit it.
                    {
                        await dbContextTransaction.CommitAsync();
                    }
                }
            }
            catch (Exception exception)
            {
                // Throws an exception.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }

        }

        /// <summary>
        /// Deletes any dashboards that are not included in the dashboard Id's.
        /// </summary>
        /// <param name="updatedDashboardIds">A list of all the dashboards that have been updated.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteMissingTwitterRuleAsync(List<int> updatedDashboardIds)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteMissingTwitterRuleAsync");

            try
            {
                // Get the dashboards that aren't included in the list of dashboards provided.
                var dashboardsToDelete = await _blashDbContext.DashboardEntities.Where(dashboard => !updatedDashboardIds.Any(updatedId => updatedId == dashboard.Id)).ToListAsync();

                dashboardsToDelete.ForEach(dashboard =>
                {
                    // Mark each one as deleted.
                    _blashDbContext.Entry(dashboard).State = EntityState.Deleted;
                });
                await _blashDbContext.SaveChangesAsync(); // Save changes.
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
