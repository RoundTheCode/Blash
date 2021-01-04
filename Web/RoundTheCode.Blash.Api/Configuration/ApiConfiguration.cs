using Newtonsoft.Json;
using RoundTheCode.Blash.TwitterApi.Configuration;
using System;
using System.Collections.Generic;

namespace RoundTheCode.Blash.Api.Configuration
{
    /// <summary>
    /// Stores the API settings which are retrieved from appsettings.json.
    /// </summary>
    public class ApiConfiguration
    {
        /// <summary>
        /// A collection of Twitter API settings, such as the client ID and secret.
        /// </summary>
        public TwitterApiConfiguration TwitterApi { get; init; }

        /// <summary>
        /// A collection of Tweet settings which are retrieved from appsettings.json.
        /// </summary>
        public TweetConfiguration Tweets { get; init; }

        /// <summary>
        /// Get a list of all the client host's that use this API.
        /// </summary>
        public IList<string> ClientHosts { get; init; }
    }
}
