using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundTheCode.Blash.Shared.Logging.FileLoggerObjects
{
    /// <summary>
    /// Stores all the file logger options.
    /// </summary>
    public class FileLoggerOptions
    {
        /// <summary>
        /// The file path of the log file stored in appsettings.json. Can use {date} to store the date in the log file.
        /// </summary>
        public virtual string FilePath { get; set; }

        /// <summary>
        /// The full file path of where the log files will be located.
        /// </summary>
        public virtual string FolderPath { get; set; }
    }
}
