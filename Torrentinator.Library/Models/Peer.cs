using System;
using System.Collections.Generic;
using System.Text;

namespace Torrentinator.Library.Models
{
    public class Peer
    {
        public Uri ConnectionUri { get; set; }
        public int RequestingPieces { get; set; }
        public int DownloadSpeed { get; set; }
        public int UploadSpeed { get; set; }
    }
}
