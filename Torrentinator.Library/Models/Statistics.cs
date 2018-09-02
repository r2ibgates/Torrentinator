using System;
using System.Collections.Generic;
using System.Text;

namespace Torrentinator.Library.Models
{
    public class Statistics
    {
        public GlobalStatistics Global { get; internal set; } = new GlobalStatistics();
        public Dictionary<string, TorrentStatistics> Torrents { get; } = new Dictionary<string, TorrentStatistics>();        
    }
}
