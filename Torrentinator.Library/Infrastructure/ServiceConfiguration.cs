using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            var ts = new TorrentService(torrentServiceOptions);
            ts.Connect();
            services.AddSingleton<ITorrentService>(p => ts);
            services.AddTransient<IDataService>(p => new DataService(dataServiceOptions));
            services.AddTransient<ITorrentRepository, TorrentRepository>();

            return services;
        }
    }
}
