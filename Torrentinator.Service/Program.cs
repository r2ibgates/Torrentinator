using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Hosting;
using Torrentinator.Library.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Torrentinator.Library.Infrastructure;
using Microsoft.Extensions.Logging;
using Torrentinator.Library.Services;

namespace Torrentinator.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            
            var serviceProvider = new ServiceCollection()
                .AddLogging(cfg => cfg.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug)
                .AddTorrentServices(config)                
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();
            
            logger.LogDebug("Starting application");

            var torrentRepo = serviceProvider.GetService<ITorrentRepository>();
            var torrentSvc = serviceProvider.GetService<ITorrentService>();

            logger.LogDebug(torrentSvc.Connected.ToString());

            logger.LogDebug("All done!");

            Console.ReadLine();
        }

    }
}
