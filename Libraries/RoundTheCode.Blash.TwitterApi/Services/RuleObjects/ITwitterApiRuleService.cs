using System.Collections.Generic;
using System.Threading.Tasks;
using RoundTheCode.Blash.TwitterApi.Data.RuleObjects;
using RoundTheCode.Blash.TwitterApi.Results.RuleObjects;

namespace RoundTheCode.Blash.TwitterApi.Services.RuleObjects
{
    /// <summary>
    /// A service for Twitter API rules.
    /// </summary>
    public interface ITwitterApiRuleService
    {
        /// <summary>
        /// Method to create a Twitter API rule.
        /// </summary>
        /// <param name="ruleEntries">A list of rule entries to be created.</param>
        /// <returns>An instance of <see cref="RuleResult"/>, which lists all the rules created, along with their unique identifier.</returns>
        Task<RuleResult> CreateRuleAsync(List<RuleEntry> ruleEntries);

        /// <summary>
        /// Get a list of all the Twitter API rules.
        /// </summary>
        /// <returns>An instance of <see cref="RuleResult"/>, which lists all the rules.</returns>
        Task<RuleResult> GetStreamRulesAsync();

        /// <summary>
        /// Deletes rules from the Twitter API.
        /// </summary>
        /// <param name="ruleIds">A list of all the rule IDs to be deleted.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        Task DeleteRuleAsync(List<string> ruleIds);
    }
}
