using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RoundTheCode.Blash.Shared.Logging.Extensions
{
    /// <summary>
    /// Stores the extensions used from writing log entires.
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Used to write an log entry.
        /// </summary>
        /// <param name="logger">An instance of <see cref="Logger"/>.</param>
        /// <param name="logLevel">An instance of <see cref="LogLevel"/>.</param>
        /// <param name="message">The log entry's message.</param>
        /// <param name="parameters">The log entry's parameters. This includes things like the method name and parameters and is used as part of the log entry's message.</param>
        public static void LogWithParameters(this ILogger logger, LogLevel logLevel, string message, Dictionary<string, object> parameters)
        {
            LogWithParameters(logger, logLevel, null, message, parameters);
        }

        /// <summary>
        /// Used to write an log entry.
        /// </summary>
        /// <param name="logger">An instance of <see cref="Logger"/>.</param>
        /// <param name="logLevel">An instance of <see cref="LogLevel"/>.</param>
        /// <param name="exception">The log entry's exception.</param>
        /// <param name="message">The log entry's message.</param>
        /// <param name="parameters">The log entry's parameters. This includes things like the method name and parameters and is used as part of the log entry's message.</param>
        public static void LogWithParameters(this ILogger logger, LogLevel logLevel, Exception exception, string message, Dictionary<string, object> parameters)
        {            
            List<object> parametersArray = new List<object>();
            var paramLog = "";

            if (parameters != null && parameters.Count > 0)
            {
                // Prepend the parameters to the log entry's message.
                var count = 0;
                paramLog += "["; // Start the param log with a '['
                foreach (var param in parameters)
                {
                    count++;
                    if (count > 1)
                    {
                        paramLog += " | "; // Add a pipe seperator between each param.
                    }
                    parametersArray.Add(string.Format("{0}: {1}", param.Key, param.Value.ToString()));  // Add the key and value to parametersArray
                    paramLog += "{Param" + count + "}"; // Add the placeholder.
                }
                paramLog += "] :: "; // End the param log with a ']' and two ':'.
            }

            // Log the entry with parameters and the message.
            logger.Log(logLevel, exception, string.Format("{0}{1}", paramLog, message), parametersArray.ToArray());
        }
    }
}
