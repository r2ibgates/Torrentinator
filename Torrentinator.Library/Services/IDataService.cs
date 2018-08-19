using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.Models;

namespace Torrentinator.Library.Services
{
    public interface IDataService
    {
        Task<IEnumerable<Torrent>> GetTorrents(Expression<Func<Torrent, bool>> filter);
        Task<Torrent> GetTorrent(string id);
        Task AddTorrent(Torrent torrent);
        Task AddTorrents(IEnumerable<Torrent> torrents);
        Task<IEnumerable<Torrent>> GetTorrentsToAdd(IEnumerable<Torrent> torrents);        
        Task<long> DeleteTorrents(Expression<Func<Torrent, bool>> filter);
        Task<Torrent> UpdateTorrent(Torrent torrent);
    }
}
