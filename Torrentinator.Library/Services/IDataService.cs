using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.Models;

namespace Torrentinator.Library.Services
{
    public interface IDataService
    {
        Task<IEnumerable<Torrent>> GetTorrents();
        Task AddTorrent(Torrent torrent);
        Task AddTorrents(IEnumerable<Torrent> torrents);
        Task<IEnumerable<Torrent>> GetTorrentsToAdd(IEnumerable<Torrent> torrents);
        Task DeleteTorrent(string id);
        Task<long> DeleteAllTorrents();
    }
}
