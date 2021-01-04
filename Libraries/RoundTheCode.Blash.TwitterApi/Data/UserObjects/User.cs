using Newtonsoft.Json;

namespace RoundTheCode.Blash.TwitterApi.Data.UserObjects
{
    /// <summary>
    /// The user object that gets used when retrieving information about the tweet's author.
    /// </summary>
    public class User
    {
        /// <summary>
        /// A unique key to identify the user.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The user's profile image URL.
        /// </summary>
        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// The user's username (or Twitter handle).
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// The user's full name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }


    }
}
