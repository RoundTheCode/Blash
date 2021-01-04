using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.Shared.Logging.FileLoggerObjects
{
    /// <summary>
    /// Creates a file logger provider. Uses "File" in the appsettings.json to retrieve the options.
    /// </summary>
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider
    {
        /// <summary>
        /// An instance of the different file logger options.
        /// </summary>
        public readonly FileLoggerOptions Options;

        /// <summary>
        /// Used to ensure that only one thread can write to a log file at any one time.
        /// </summary>
        public SemaphoreSlim WriteFileLock;

        /// <summary>
        /// A new instance of <see cref="FileLoggerProvider"></cref>, passing in the file logger options.
        /// </summary>
        /// <param name="_options"></param>
        public FileLoggerProvider(IOptions<FileLoggerOptions> _options)
        {
            WriteFileLock = new SemaphoreSlim(1, 1); // New instance of the lock.
            Options = _options.Value; // Stores all the options.

            // Ensure the log file's directory is created if it does not exist.
            if (!Directory.Exists(Options.FolderPath))
            {
                Directory.CreateDirectory(Options.FolderPath);
            }
        }

        /// <summary>
        /// Creates a new instance of the file logger.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(this);
        }

        /// <summary>
        /// Disposes of the <see cref="FileLoggerProvider"></cref> instance.
        /// </summary>
        public void Dispose()
        {
            if (WriteFileLock != null)
            {
                WriteFileLock.Dispose(); // Disposes the file lock.
            }
        }
    }
}
