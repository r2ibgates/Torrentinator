using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Torrentinator.Library.Models
{
    internal class TorrentData : Torrent
    {
        [BsonId]
        public long Id { get; set; }

        public TorrentData(): base() { }
        internal TorrentData(Torrent torrent): base()
        {
            torrent.Copy(this);
            var idNumber = System.Text.RegularExpressions.Regex.Match(this.TorrentId, "\\/([\\d]+)\\/").Groups[1].Value;
            this.Id = long.Parse(idNumber);
        }
    }
}
