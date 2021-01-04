using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoundTheCode.Blash.TwitterApi.Configuration;
using RoundTheCode.Blash.TwitterApi.Services;
using RoundTheCode.Blash.TwitterApi.Services.AuthenticateObjects;
using RoundTheCode.Blash.TwitterApi.Services.RuleObjects;
using RoundTheCode.Blash.TwitterApi.Services.TweetObjects;

namespace RoundTheCode.Blash.TwitterApi.Extensions
{
    /// <summary>
    /// A list of Twitter API extensions.
    /// </summary>
    public static class TwitterApiExtensions
    {
        /// <summary>
        /// A list of services to add to the <see cref="IServiceCollection" /> when configuration the application.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="twitterApiConfigurationSection"></param>
        /// <returns></returns>
        public static IServiceCollection AddTwitterApiServices(this IServiceCollection services, IConfigurationSection twitterApiConfigurationSection)
        {
            services.Configure<TwitterApiConfiguration>(twitterApiConfigurationSection);
            services.AddSingleton<ITwitterApiAuthenticateService, TwitterApiAuthenticateService>();
            services.AddSingleton<ITwitterApiRuleService, TwitterApiRuleService>();
            services.AddSingleton<ITwitterApiTweetService, TwitterApiTweetService>();

            return services;
        }
    }
}
