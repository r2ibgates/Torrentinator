﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Torrentinator.Library.Models;
using Torrentinator.Types;

namespace Torrentinator.Models
{
    public class TorrentViewModel : Torrent
    {
        [UIHint("BytesFormat")]
        public long Bytes => this.Length;
        public Progress DownloadProgress { get;  } = new Progress();

        internal static TorrentViewModel Create(Torrent torrent)
        {
            return new TorrentViewModel()
            {
                TorrentId = torrent.TorrentId,
                Title = torrent.Title,
                Description = torrent.Description,
                Length = torrent.Length,
                Published = torrent.Published,
                Url = torrent.Url
            };
        }
    }
}
