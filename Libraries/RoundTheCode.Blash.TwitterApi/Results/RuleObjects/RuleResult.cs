using Newtonsoft.Json;
using System.Collections.Generic;
using RoundTheCode.Blash.TwitterApi.Data.RuleObjects;

namespace RoundTheCode.Blash.TwitterApi.Results.RuleObjects
{
    /// <summary>
    /// The stored object when retrieving a list of rules from the Twitter API.
    /// </summary>
    public class RuleResult
    {
        /// <summary>
        /// A list of rule entries returned.
        /// </summary>
        [JsonProperty("data")]
        public List<RuleEntry> RuleEntries { get; set; }
    }
}
