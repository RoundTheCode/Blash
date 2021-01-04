using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using RoundTheCode.Blash.Shared.Logging.FileLoggerObjects;

namespace RoundTheCode.Blash.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Use the start up.
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostBuilderContext, logger) =>
                {
                    // Add the ability to log files to text files.
                    logger.AddFileLogger(options =>
                    {
                        hostBuilderContext.Configuration.GetSection("Logging").GetSection("File").GetSection("Options").Bind(options);
                    });
                })
            ;
    }
}
