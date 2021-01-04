using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.TwitterApi.Services.AuthenticateObjects
{
    /// <summary>
    /// A service for Twitter API authentication.
    /// </summary>
    public interface ITwitterApiAuthenticateService
    {
        /// <summary>
        /// Gets the OAuth2 token which can be used when calling Twitter API methods.
        /// </summary>
        /// <returns>The OAuth2 token.</returns>
        Task<string> GetOAuth2TokenAsync();
    }
}
