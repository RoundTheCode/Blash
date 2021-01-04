using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RoundTheCode.Blash.Api.Exceptions;
using RoundTheCode.Blash.Api.Services;
using RoundTheCode.Blash.Data.Data.DashboardObjects;
using Tweet = RoundTheCode.Blash.Data.Data.DashboardObjects.Tweet;

namespace RoundTheCode.Blash.Api.Background.Tasks.Extensions
{
    /// <summary>
    /// Extensions used when running tasks.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Create, or update a tweet, based on what is passed back from the Twitter API.
        /// </summary>
        /// <param name="scope">The current scope.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used for writing logs.</param>
        /// <param name="apiTweet">An instance of a tweet retrieved from the Twitter API.</param>
        /// <param name="users">An instance of the users for a tweet, retrieved from the Twitter API.</param>
        /// <param name="media">An instance of the media for a tweet, retrieved from the Twitter API.</param>
        /// <returns>A tweet instance that has been created or updated against the Blash database.</returns>
        public static async Task<Tweet> CreateUpdateTweetAsync(IServiceScope scope, ILogger logger, TwitterApi.Data.TweetObjects.Tweet apiTweet, List<TwitterApi.Data.UserObjects.User> users, List<TwitterApi.Data.MediaObjects.Media> media)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Static Class", "TaskExtensions");
            parameters.Add("Method", "CreateUpdateTweetAsync");
            parameters.Add("Twitter API Tweet Id", apiTweet.Id);

            try
            {
                // Get the services from the scope.
                var authorService = scope.ServiceProvider.GetService<IAuthorService>();
                var dashboardService = scope.ServiceProvider.GetService<IDashboardService>();
                var tweetService = scope.ServiceProvider.GetService<ITweetService>();

                if (apiTweet.ReferencedTweets != null && apiTweet.ReferencedTweets.Count > 0)
                {
                    // Not interested in referenced tweets.
                    return null;
                }

                // Does author exist in database?
                var author = await authorService.GetByTwitterAuthorAsync(apiTweet.AuthorId);

                var authorUser = users.FirstOrDefault(user => user.Id == apiTweet.AuthorId);

                if (authorUser == null)
                {
                    // Throw exception if we can't find the tweet user in the list of users passed in as a parameter.
                    throw new ApiException(string.Format("Cannot find the author that references '{0}'", author.Id));
                }
              
                var authorCreate = author == null;

                if (authorCreate)
                {
                    // Create an instance of author, if it doesn't exist in the database.
                    author = new Author();
                }

                // Update author properties.
                author.TwitterAuthorId = authorUser.Id;
                author.TwitterName = authorUser.Name;
                author.TwitterHandle = authorUser.Username;
                author.TwitterImageUrl = authorUser.ProfileImageUrl;

                if (authorCreate)
                {
                    // Create author if it doesn't exist in the database.
                    await authorService.CreateAsync(author);
                }
                else
                {
                    // Or, update the author if it does exist in the database.
                    await authorService.UpdateAsync(author.Id, author);
                }


                // Add the tweet if it doesn't exist.
                var tweet = await tweetService.GetByTwitterTweetAsync(apiTweet.Id);

                var tweetCreate = tweet == null;

                if (tweetCreate)
                {
                    // Create an instance of tweet, if it doesn't exist in the database.
                    tweet = new Tweet();
                }

                // Update tweet properties.

                tweet.AuthorId = author.Id;
                tweet.TwitterTweetId = apiTweet.Id;
                tweet.Content = ConvertMediaAndLinks(apiTweet, media);
                tweet.TwitterPublished = apiTweet.Created;

                if (tweetCreate)
                {
                    // Create tweet if it doesn't exist in the database.
                    await tweetService.CreateAsync(tweet);
                }
                else
                {
                    // Or, update the tweet if it does exist in the database.
                    await tweetService.UpdateAsync(tweet.Id, tweet);
                }

                // Log that it has been imported into the database.
                logger.Log(LogLevel.Debug, string.Format("Tweet has been imported to the Tweet table in the database (id: '{0}')", tweet.Id), parameters);

                // Return the instance of tweet.
                return tweet;
            }
            catch (ApiException exception)
            {
                // Log any API exceptions.
                logger.Log(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
            catch (Exception exception)
            {
                // Log other exceptions.
                logger.Log(LogLevel.Error, exception, exception.Message, parameters);
                throw;
            }
        }

        /// <summary>
        /// Create or update the relationship between a dashboard and a tweet.
        /// </summary>
        /// <param name="scope">The current scope.</param>
        /// <param name="logger">An instance of <see cref="ILogger"/>, used for writing logs.</param>
        /// <param name="dashboardId">The dashboard's identifier from the Blash database.</param>
        /// <param name="tweetId">The tweet's identifier from the Blash database.</param>
        /// <returns>An instance of the database record relationship between the dashboard and the tweet.</returns>
        public static async Task<DashboardTweet> CreateDashboardTweetRelationshipAsync(IServiceScope scope, ILogger logger, int dashboardId, int tweetId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add("Static Class", "TaskExtensions");
            parameters.Add("Method", "CreateDashboardTweetRelationshipAsync");
            parameters.Add("DB Tweet Id", tweetId);

            try
            {
                var dashboardTweetService = scope.ServiceProvider.GetService<IDashboardTweetService>();

                // Does the dashboard and tweet exist as a relationship?
                var dashboardTweet = await dashboardTweetService.GetByDashboardTweetAsync(dashboardId, tweetId);

                if (dashboardTweet == null)
                {
                    // No relationship, so add it.
                    dashboardTweet = await dashboardTweetService.CreateAsync(new DashboardTweet
                    {
                        DashboardId = dashboardId,
                        TweetId = tweetId
                    });
                }

                // Log that the tweet has been added to the dashboard.
                logger.Log(LogLevel.Debug, string.Format("Tweet has been added to Dashboard (id: '{0}').", dashboardId), parameters);

                return dashboardTweet;
            }
            catch (Exception exception)
            {
                // Logs any exceptions.
                logger.Log(LogLevel.Error, exception, "Unable to complete method due to an exception", parameters);
                throw;
            }
        }

        /// <summary>
        /// Add any images into a tweet.
        /// </summary>
        /// <param name="twitterApiTweet">An instance of the tweet retrieved from the Twitter API.</param>
        /// <param name="twitterApiMedia">Any media that belongs to a tweet, retreived from the Twitter API.</param>
        /// <returns></returns>
        private static string ConvertMediaAndLinks(TwitterApi.Data.TweetObjects.Tweet twitterApiTweet, List<TwitterApi.Data.MediaObjects.Media> twitterApiMedia)
        {
            if (twitterApiTweet == null || string.IsNullOrWhiteSpace(twitterApiTweet.Text))
            {
                // If there is no tweet, or no text, return empty.
                return null;
            }

            if (twitterApiTweet.Attachments != null && twitterApiTweet.Attachments.MediaKeys != null && twitterApiTweet.Attachments.MediaKeys.Count() > 0 && twitterApiMedia != null && twitterApiMedia.Count() > 0)
            {
                var images = string.Empty;
                foreach (var mediaKey in twitterApiTweet.Attachments.MediaKeys)
                {
                    // Go through each media key and see if it appears in the media instance.
                    var media = twitterApiMedia.FirstOrDefault(media => media.MediaKey == mediaKey);

                    if (media == null || media.Type != "photo")
                    {
                        // Not interested if it's not a photo.
                        continue;
                    }

                    // Add the image as a <img> tag in the images variable.
                    images += string.Format("\r\n<img src=\"{0}\" alt=\"{1}\" style=\"max-height: 200px; max-width:100%; height: auto\" />", media.Url, media.MediaKey);
                }

                if (!string.IsNullOrWhiteSpace(images))
                {
                    // If images exist, remove the last https://t.co from the tweet, and replace it with the images.
                    var imageRegex = new Regex(@"( )?https:\/\/t\.co\/([a-z0-9A-Z]+)$", RegexOptions.Singleline);

                    if (imageRegex.IsMatch(twitterApiTweet.Text))
                    {
                        twitterApiTweet.Text = imageRegex.Replace(twitterApiTweet.Text, "");
                    }

                    twitterApiTweet.Text += images;
                }
            }

            // Convert any remaining links into a <a> tag, so they are clickable on the dashboard.
            var linkRegex = new Regex(@"(https:\/\/t\.co\/([a-z0-9A-Z]+))");

            if (linkRegex.IsMatch(twitterApiTweet.Text))
            {
                twitterApiTweet.Text = linkRegex.Replace(twitterApiTweet.Text, "<a href=\"$1\">$1</a>");
            }

            return twitterApiTweet.Text;
        }

    }
}
