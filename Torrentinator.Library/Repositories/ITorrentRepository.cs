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
    }
}
