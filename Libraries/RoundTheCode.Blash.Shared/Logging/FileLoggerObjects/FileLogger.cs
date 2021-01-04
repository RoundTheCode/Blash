using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.Shared.Logging.FileLoggerObjects
{
    /// <summary>
    /// Writes a log entry to a text file.
    /// </summary>
    public class FileLogger : ILogger
    {
        /// <summary>
        /// Instance of <see cref="FileLoggerProvider" />.
        /// </summary>
        protected readonly FileLoggerProvider _fileLoggerProvider;

        /// <summary>
        /// Creates a new instance of <see cref="FileLogger" />.
        /// </summary>
        /// <param name="fileLoggerProvider">Instance of <see cref="FileLoggerProvider" />.</param>
        public FileLogger([NotNull] FileLoggerProvider fileLoggerProvider)
        {
            _fileLoggerProvider = fileLoggerProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Whether to log the entry.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }


        /// <summary>
        /// Used to log the entry.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel">An instance of <see cref="LogLevel"/>.</param>
        /// <param name="eventId">The event's ID. An instance of <see cref="EventId"/>.</param>
        /// <param name="state">The event's state.</param>
        /// <param name="exception">The event's exception. An instance of <see cref="Exception" /></param>
        /// <param name="formatter">A delegate that formats </param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                // Don't log the entry if it's not enabled.
                return;
            }

            var threadId = Thread.CurrentThread.ManagedThreadId; // Get the current thread ID to use in the log file.

            Task.Run(async() =>
            {
                // Run in a seperate task so the thread isn't waiting for it to be finished.
                var fullFilePath = _fileLoggerProvider.Options.FolderPath + "/" + _fileLoggerProvider.Options.FilePath.Replace("{date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd")); // Get the full log file path. Seperated by day.
                var logRecord = string.Format("{0} [{1}] [{2}] {3} {4}", "[" + DateTimeOffset.UtcNow.ToString("yyyy-MM-dd\\THH:mm:ss.fff\\Z") + "]", threadId, logLevel.ToString(), formatter(state, exception), exception != null ? exception.StackTrace : ""); // Format the log entry.

                try
                {
                    // Ensure that only one thread can write to the text file to avoid issues with opening the text file.
                    await _fileLoggerProvider.WriteFileLock.WaitAsync();

                    // Write the log entry to the text file.
                    using (var streamWriter = new StreamWriter(fullFilePath, true))
                    {
                        await streamWriter.WriteLineAsync(logRecord);
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    // Ensure that the lock is released once the log entry has been written to the text file.
                    _fileLoggerProvider.WriteFileLock.Release();
                }
            });
        }
    }
}
