using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.Models;

namespace Torrentinator.Library.Repositories
{
    public interface ITorrentRepository
    {
        Task ImportTorrents();
        Task DeleteTorrent(string id);
        Task DeleteAllTorrents();
        Task<IEnumerable<Torrent>> GetTorrents();
        Task<Torrent> SetStatus(string id, TorrentStatus status);
        Task<Torrent> SetDownloadProgress(string id, long progress);
        Task<IEnumerable<Torrent>> GetTorrentsForStatus(TorrentStatus status);
        Task<Torrent> GetTorrent(string id);
        Task StartDownload(string id);
    }
}
