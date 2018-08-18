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
            var newTorrents = await DataService.GetTorrentsToAdd(torrents.Select(t => new Torrent()
            {
                TorrentId = t.Id,
                Title = t.Title,
                Description = t.Description,
                Length = t.Length,
                Published = t.Published,
                Url = t.Magnet
            }));
            var tds = new List<Torrent>();

            foreach(var t in newTorrents.Take(10))
            {
                t.Description = await TorrentService.GetDescription(t.TorrentId) ?? t.Description;
                tds.Add(t);
            }
            await DataService.AddTorrents(tds);
        }

        public async Task DeleteTorrent(string id)
        {
            await DataService.DeleteTorrent(id);
        }
        public async Task DeleteAllTorrents()
        {
            await DataService.DeleteAllTorrents();
        }

        public async Task<IEnumerable<Torrent>> GetTorrents()
        {
            return await DataService.GetTorrents();
        }
    }
}
