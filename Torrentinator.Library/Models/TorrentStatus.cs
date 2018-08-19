using System;
using System.Collections.Generic;
using System.Text;

namespace Torrentinator.Library.Models
{
    public enum TorrentStatus
    {
        Ready = 0,
        Downloading = 1,
        Complete = 2,
        Disabled = 3
    }
}
