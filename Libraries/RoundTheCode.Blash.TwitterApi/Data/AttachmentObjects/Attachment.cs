using Newtonsoft.Json;
using System.Collections.Generic;

namespace RoundTheCode.Blash.TwitterApi.Data.AttachmentObjects
{
    /// <summary>
    /// The Attachment object from the Twitter API.
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Gets a list of media keys stored in the object.
        /// </summary>
        [JsonProperty("media_keys")]
        public List<string> MediaKeys { get; set; }
    }
}
