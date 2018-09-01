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
using Serilog;

namespace Torrentinator.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Run app
            var torrentSvc = serviceProvider.GetService<ITorrentService>();
            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger.LogDebug("Starting application");
            logger.LogDebug(torrentSvc.Connected.ToString());
            logger.LogDebug("All done!");

            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Add logging
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddConsole()
                .AddSerilog()
                .AddDebug());
            serviceCollection.AddLogging();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();            

            // Initialize serilog logger
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.ColoredConsole()
                 .WriteTo.RollingFile(".\\logs\\Torrentinator.log",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                 .MinimumLevel.Debug()
                 .Enrich.FromLogContext()
                 .CreateLogger();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton(configuration);

            // Add services
            serviceCollection.AddTorrentServices(configuration);
        }
    }
}
