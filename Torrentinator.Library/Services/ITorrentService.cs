using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.Models;
using Torrentinator.Library.RSS;
using Torrentinator.Library.Types;
using static Torrentinator.Library.Services.TorrentService;

namespace Torrentinator.Library.Services
{
    public interface ITorrentService
    {
        event EventHandler<TorrentHash> PieceHashed;
        event EventHandler<Statistics> StatisticsUpdated;

        void Disconnect();
        bool Connected { get; }
        string ConnectionError { get; }
        string Address { get; }
        int SocksPort { get; }
        int ControlPort { get; }
        string TorIP { get; }
        string CurrentTorIP { get; }
        TorConnectResult Connect();
        Task<IEnumerable<TorrentRSSItem>> GetTorrentsFromRSS();
        Task<string> GetDescription(string torrentId);
        void StartDownload(Torrent torrent);
    }
}
