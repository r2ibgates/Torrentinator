using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.RSS;
using Torrentinator.Library.Types;

namespace Torrentinator.Library.Services
{
    public interface ITorrentService
    {
        void Disconnect();
        Task<TorConnectResult> Connect();
        Task<IEnumerable<TorrentRSSItem>> GetTorrentsFromRSS();
    }
}
