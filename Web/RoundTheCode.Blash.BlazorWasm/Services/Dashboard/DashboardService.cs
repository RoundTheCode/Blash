
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RoundTheCode.Blash.BlazorWasm.Services.Api;
using RoundTheCode.Blash.Data.Models;
using RoundTheCode.Blash.Data.Results;

namespace RoundTheCode.Blash.BlazorWasm.Services.Dashboard
{
    /// <summary>
    /// A service used with communication with the API when performing dashboard tasks.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        protected readonly IApiService _apiService;

        /// <summary>
        /// Creates a new instance of <see cref="DashboardService"/>.
        /// </summary>
        /// <param name="apiService">An instance of <see cref="IApiService"/>.</param>
        public DashboardService([NotNull] IApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Creates a dashboard with the API.
        /// </summary>
        /// <param name="dashboardModel">An instance of <see cref="DashboardModel"/>.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task CreateDashboardAsync(DashboardModel dashboardModel)
        {
            var url = string.Format("{0}/api/dashboard", _apiService.ApiHost); // Get the API url.

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    // POST Request. Use the model as a JSON request.
                    request.Content = new StringContent(JsonConvert.SerializeObject(dashboardModel), Encoding.UTF8, "application/json");

                    // Sends the request and gets the response from the API.
                    using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response == null)
                        {
                            // Throw an exception if no response.
                            throw new Exception("Unable to get a response from the Twitter API");
                        }

                        if (!((int)response.StatusCode).ToString().StartsWith("2"))
                        {
                            // Throw an exception if error.
                            throw new Exception(string.Format("The API returned a '{0}' response.", (int)response.StatusCode));
                        }
                    }

                }
            }

        }

        /// <summary>
        /// Gets a list of all the dashboards and their tweets.
        /// </summary>
        /// <returns>A list of dashboards and their tweets.</returns>
        public async Task<List<DashboardAndTweetsResult>> GetDashboardAndTweetsAsync()
        {
            var url = string.Format("{0}/api/client/dashboard-tweets", _apiService.ApiHost); // Get the API url.

            List<DashboardAndTweetsResult> dashboardAndTweets = null;

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {               
                    // Sends the request and gets the response from the API.
                    using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response == null)
                        {
                            // Throw an exception if no response.
                            throw new Exception("Unable to get a response from the Twitter API");
                        }

                        var content = await response.Content.ReadAsStringAsync(); // Reads the response's content.

                        if (!((int)response.StatusCode).ToString().StartsWith("2"))
                        {
                            // Throw an exception if error.
                            throw new Exception(string.Format("The API returned a '{0}' response.", (int)response.StatusCode));
                        }

                        try
                        {
                            var jsonContent = JObject.Parse(content); // Convert to JSON.
                            dashboardAndTweets = JsonConvert.DeserializeObject<List<DashboardAndTweetsResult>>(jsonContent["result"].ToString()); // Derserailise the object into a list of Dashboard & Tweets.
                        }
                        catch (Exception exception)
                        {
                            // Throw an exception.
                            throw new Exception("Unable to convert the JSON response to a DashboardAndTweets type.", exception);
                        }
                    }
                }
            }
            return dashboardAndTweets;
        }

        /// <summary>
        /// Deletes a dashboard.
        /// </summary>
        /// <param name="dashboardId">The dashboard identifier.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task DeleteAsync(int dashboardId)
        {
            var url = string.Format("{0}/api/dashboard/{1}", _apiService.ApiHost, dashboardId); // Get the API url.

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Delete, url)) // Performs a DELETE request.
                {
                    // Sends the request and gets the response from the API.
                    using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response == null)
                        {
                            // Throw an exception if no response.
                            throw new Exception("Unable to get a response from the Twitter API");
                        }

                        if (!((int)response.StatusCode).ToString().StartsWith("2"))
                        {
                            // Throw an exception.
                            throw new Exception(string.Format("The API returned a '{0}' response.", (int)response.StatusCode));
                        }
                    }
                }
            }

        }
    }
}
