using OctoTorrent.BEncoding;
using OctoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            await DataService.AddTorrents(newTorrents);
        }

        public async Task DeleteTorrent(string id)
        {
            await DataService.DeleteTorrents(t => t.TorrentId == id);
        }
        public async Task DeleteAllTorrents()
        {
            await DataService.DeleteTorrents(null);
        }

        public async Task<IEnumerable<Torrent>> GetTorrents()
        {
            return await DataService.GetTorrents(null);
        }
        public async Task<Torrent> GetTorrent(string id)
        {
            return await DataService.GetTorrent(id);
        }

        public async Task<Torrent> SetStatus(string id, TorrentStatus status)
        {
            var torrent = await DataService.GetTorrent(id);
            torrent.Status = status;
            await DataService.UpdateTorrent(torrent);
            return torrent;
        }

        public async Task<Torrent> SetDownloadProgress(string id, long progress)
        {
            var torrent = await DataService.GetTorrent(id);
            torrent.Downloaded = progress;
            if (torrent.Downloaded > 0)
                torrent.Status = TorrentStatus.Downloading;
            else if (torrent.Downloaded == torrent.Length)
                torrent.Status = TorrentStatus.Complete;
            else
                torrent.Status = TorrentStatus.Ready;
            return torrent;
        }

        public async Task<IEnumerable<Torrent>> GetTorrentsForStatus(TorrentStatus status)
        {
            return await DataService.GetTorrents(t => t.Status == status);
        }

        public async Task StartDownload(string torrentId)
        {
            var torrent = await this.GetTorrent(torrentId);
            this.TorrentService.StatisticsUpdated += delegate (object o, Statistics stats)
            {
                Console.Clear();
                foreach(var tstat in stats.Torrents.Values)
                {
                    Console.WriteLine($"{tstat.Name} Downloaded {tstat.BytesDownloaded.ToString("#,##0")} at {(tstat.DownloadSpeed / 1024.0).ToString("#,##0.0")}kB/s {(tstat.Progress / 100.0).ToString("P2")}");
                }
            };
            this.TorrentService.StartDownload(torrent);
        }
    }
}
