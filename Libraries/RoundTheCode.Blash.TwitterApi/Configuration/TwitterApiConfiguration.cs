using Newtonsoft.Json;
using System;

namespace RoundTheCode.Blash.TwitterApi.Configuration
{
    /// <summary>
    /// Stores the Twitter API settings which are retrieved from appsettings.json.
    /// </summary>
    public class TwitterApiConfiguration
    {
        /// <summary>
        /// The Client ID used to connect to the Twitter API.
        /// </summary>
        public string ClientId { get; init; }

        /// <summary>
        /// The Client Secret used to connect to the Twitter API.
        /// </summary>
        public string ClientSecret { get; init; }
    }
}
