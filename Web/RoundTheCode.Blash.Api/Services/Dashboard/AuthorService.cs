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
    /// A service used for the <see cref="Author"/> entity, used to supply CRUD methods against <see cref="BlashDbContext"/>.
    /// </summary>
    public class AuthorService : BaseService<Author>, IAuthorService
    {
        /// <summary>
        /// Creates a new instance of <see cref="AuthorService"/>.
        /// </summary>
        /// <param name="blashDbContext">An instance of <see cref="BlashDbContext"/>.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used to write logs.</param>
        public AuthorService(BlashDbContext blashDbContext, ILogger<AuthorService> logger) : base(blashDbContext, logger) { }

        /// <summary>
        /// Gets an author record from the <see cref="BlashDbContext"/> by passing in the Twitter API Author ID.
        /// </summary>
        /// <param name="twitterAuthorId">The author identifier from the Twitter API.</param>
        /// <returns>The author record returned from <see cref="BlashDbContext"/>, (or null if it cannot be found).</returns>
        public async Task<Author> GetByTwitterAuthorAsync(string twitterAuthorId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "GetByTwitterAuthorAsync");
            parameters.Add("Twitter API Author Id", twitterAuthorId);

            try
            {
                // Get the record based on the Twitter API author identifier. Returns null if not found.
                return await _blashDbContext.AuthorEntities.FirstOrDefaultAsync(author => author.TwitterAuthorId == twitterAuthorId);
            }
            catch (Exception exception)
            {
                // Log any exceptions.
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Delete any author records from <see cref="BlashDbContext"/> where there are no tweets associated with it.
        /// </summary>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteMissingTweetsAsync()
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Method", "DeleteMissingTweetsAsync");

            try
            {
                // Find any authors that don't have any tweets linked to them.
                var authorToDelete = await _blashDbContext.AuthorEntities.Where(author => !_blashDbContext.TweetEntities.Any(tweet => tweet.AuthorId == author.Id)).ToListAsync();

                // Mark all authors as deleted in the DbContext.
                authorToDelete.ForEach(dashboard =>
                {
                    _blashDbContext.Entry(dashboard).State = EntityState.Deleted;
                });
                await _blashDbContext.SaveChangesAsync(); // Save the changes.
            }
            catch (Exception exception)
            {
                _logger.LogWithParameters(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

    }
}
