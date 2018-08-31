using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Torrentinator.Library.Repositories;
using Torrentinator.Library.Services;

namespace Torrentinator.Library.Infrastructure
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddTorrentServices(this IServiceCollection services, IConfiguration configuration)
        {
            var dataServiceOptions = new DataService.DataServiceOptions();
            var torrentServiceOptions = new TorrentService.TorrentServiceOptions();
            
            configuration.GetSection("DataService").Bind(dataServiceOptions);
            configuration.GetSection("TorrentService").Bind(torrentServiceOptions);

            services.AddSingleton<ITorrentService>(p =>
            {
                var ts = new TorrentService(torrentServiceOptions, p.GetService<ILoggerFactory>());
                ts.Connect();
                return ts;
            });
            services.AddTransient<IDataService>(p => new DataService(dataServiceOptions));
            services.AddTransient<ITorrentRepository, TorrentRepository>();
            
            return services;
        }
    }
}
