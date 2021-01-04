using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace RoundTheCode.Blash.Shared.Logging.FileLoggerObjects
{
    /// <summary>
    /// Stores all the File Logger's extensions.
    /// </summary>
    public static class FileLoggerExtensions
    {
        /// <summary>
        /// Services used to add to the <see cref="IServiceCollection" /> when configuring an application.
        /// </summary>
        /// <param name="builder">An instance of <see cref="ILoggingBuilder"></cref></param>
        /// <param name="configure">A instance of all the properties required for the file logger.</param>
        /// <returns></returns>
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}
