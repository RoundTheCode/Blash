using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.TwitterApi.Exceptions
{
    /// <summary>
    /// An exception used when there is an issue with any calls from the Twitter API.
    /// </summary>
    public class TwitterApiException : Exception
    {
        /// <summary>
        /// This stores the date & time when the Twitter API limit is reset, should it be returned from the Twitter API call.
        /// </summary>
        public DateTimeOffset? XRateLimitResetDate { get; init; }

        /// <summary>
        /// Initalises a new instance of <see cref="TwitterApiException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TwitterApiException(string message) : base(message) { }

        /// <summary>
        /// Initalises a new instance of <see cref="TwitterApiException" /> class with a specified error message and a reference to the inner exception that is the cause of the exception. 
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public TwitterApiException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initalises a new instance of <see cref="TwitterApiException" /> class with a specified error message, a reference to the inner exception that is the cause of the exception and when the Twitter API rate limit is reset.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="xRateLimitSet">The date for when the Twitter API rate limit expires.</param>
        public TwitterApiException(string message, Exception innerException, int? xRateLimitSet) : this(message, innerException)
        {
            if (xRateLimitSet.HasValue)
            {
                // Set the rate limit if it exists.
                XRateLimitResetDate = DateTimeOffset.FromUnixTimeSeconds(xRateLimitSet.Value);
            }
        }

        /// <summary>
        /// Initalises a new instance of <see cref="TwitterApiException" /> class with a specified error message and when the Twitter API rate limit is reset.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="xRateLimitSet">The date for when the Twitter API rate limit expires.</param>
        public TwitterApiException(string message, int? xRateLimitReset) : this(message, null, xRateLimitReset)
        {
        }
    }
}
