using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RoundTheCode.Blash.TwitterApi.Exceptions;

namespace RoundTheCode.Blash.TwitterApi.Services.Extensions
{
    /// <summary>
    /// A list of extensions used when accessing the Twitter API.
    /// </summary>
    public static class TwitterApiServiceExtensions
    {
        /// <summary>
        /// Returns a response from a Twitter API request.
        /// </summary>
        /// <param name="httpClient">An instance of <see cref="HttpClient"/>, used to perform the HTTP request to the Twitter API.</param>
        /// <param name="request">An instance of <see cref="HttpRequestMessage"/></param>, which stores all the information about the request.
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetTwitterApiResponseAsync(HttpClient httpClient, HttpRequestMessage request)
        {
            // Gets a response from the HTTP request.
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response == null)
            {
                // No response, so throw an exception.
                throw new TwitterApiException("Unable to get a response from the Twitter API.");
            }

            // Status code does not being with 2, so assume that the Twitter API has throw an exception.
            if (!((int)response.StatusCode).ToString().StartsWith("2"))
            {
                // Read error message.
                var content = await response.Content.ReadAsStringAsync();

                JObject jsonErrorContent = null;
                var errorMessages = string.Empty;

                try
                {
                    // Parse the response content as a JSON object.
                    jsonErrorContent = JObject.Parse(content);
                }
                catch (Exception)
                {
                    errorMessages += content;
                }

                if (jsonErrorContent != null) {
                    // Get the errors.
                    JToken errorJson = jsonErrorContent["errors"];

                    if (errorJson != null && errorJson.Type == JTokenType.Array)
                    {
                        if (errorJson[0]["message"] == null || errorJson[0]["message"].Type == JTokenType.Null)
                        {
                            // If there is another errors object.
                            errorJson = errorJson["errors"];
                        }

                        if (errorJson != null && errorJson.Type == JTokenType.Array)
                        {
                            // Get a list of all the errors and store them in the errorMessages instance.
                            foreach (var error in errorJson)
                            {
                                if (error["message"] != null && error["message"].Type != JTokenType.Null)
                                {
                                    errorMessages += (!string.IsNullOrWhiteSpace(errorMessages) ? "\r\n\r\n" : "") + error["message"];
                                }
                            }
                        }
                    }
                    else if (jsonErrorContent["detail"] != null && jsonErrorContent["detail"].Type != JTokenType.Null)
                    {
                        // Store information about the detail.
                        errorMessages += (!string.IsNullOrWhiteSpace(errorMessages) ? "\r\n\r\n" : "") + jsonErrorContent["detail"];
                    }

                }

                // Does response have a rate limit?
                int? xRateLimitReset = null;
                int xRate;
                if (response.Headers != null && response.Headers.Contains("x-rate-limit-reset") && int.TryParse(response.Headers.GetValues("x-rate-limit-reset").FirstOrDefault(), out xRate))
                {
                    // Store when the rate limit is reset from the Twitter API.
                    xRateLimitReset = xRate;
                }

                // Throw a new Twitter API exception, detailing the status code response.
                throw new TwitterApiException(string.Format("The API returned a '{0}' response.{1}", (int)response.StatusCode, !string.IsNullOrWhiteSpace(errorMessages) ? "The following messages returned:\r\n" + errorMessages + "\r\n" : ""), xRateLimitReset);
            }

            return response;
        }
    }
}
