using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.Api.Exceptions
{
    /// <summary>
    /// An exception used when there is an issue with any calls from the API.
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// A JSON object of the error.
        /// </summary>
        public JObject Error { get; init; }

        /// <summary>
        /// Initalises a new instance of <see cref="ApiException" /> class.
        /// </summary>
        public ApiException() : base() { }

        /// <summary>
        /// Initalises a new instance of <see cref="ApiException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ApiException(string message) : base(message) { }

        /// <summary>
        /// Initalises a new instance of <see cref="ApiException" /> class with a specified error message and a reference to the inner exception that is the cause of the exception. 
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ApiException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initalises a new instance of <see cref="ApiException" /> class with a specified error message, and a JSON object of the error.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="error">JSON object of the error.</param>
        public ApiException(string message, JObject error) : base(message)
        {
            Error = error;
        }

        /// <summary>
        /// Initalises a new instance of <see cref="ApiException" /> class with a specified error message, a reference to the inner exception that is the cause of the exception, and a JSON object of the error. 
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="error">JSON object of the error.</param>
        public ApiException(string message, Exception innerException, JObject error) : base(message, innerException)
        {
            Error = error;
        }
    }
}
