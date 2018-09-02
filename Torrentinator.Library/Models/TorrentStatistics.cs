using System;
using System.Collections.Generic;
using System.Text;

namespace Torrentinator.Library.Models
{
    public class TorrentStatistics
    {
        // public TorrentState State { get; set; }
        public string Name { get; set; }
        public double Progress { get; set; }
        public int DownloadSpeed { get; set; }
        public int UploadSpeed { get; set; }
        public long BytesDownloaded { get; set; }
        public long BytesUploaded { get; set; }
        public string WarningMessage { get; set; }
        public string FailureMessage { get; set; }
        public int CurrentRequests { get; set; }
        public List<Peer> Peers { get; } = new List<Peer>();
        public Dictionary<string, double> FilesCompleted { get; } = new Dictionary<string, double>();
    }
}
