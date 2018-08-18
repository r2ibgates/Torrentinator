using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.Models;
using Torrentinator.Library.Services;

namespace Torrentinator.Library.Repositories
{
    internal class TorrentRepository : ITorrentRepository
    {
        private readonly IDataService DataService;
        private readonly ITorrentService TorrentService;

        public TorrentRepository(IDataService dataService, ITorrentService torrentService)
        {
            this.DataService = dataService;
            this.TorrentService = torrentService;
        }

        public async Task ImportTorrents()
        {
            var torrents = await TorrentService.GetTorrentsFromRSS();
            await DataService.AddTorrents(torrents.Select(t => new Torrent()
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Length = t.Length,
                Published = t.Published,
                Url = t.Magnet
            }));
        }

        public async Task DeleteTorrent(string id)
        {
            await DataService.DeleteTorrent(id);
        }

        public async Task<IEnumerable<Torrent>> GetTorrents()
        {
            return await DataService.GetTorrents();
        }
    }
}
