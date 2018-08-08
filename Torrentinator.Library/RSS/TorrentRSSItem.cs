using Microsoft.SyndicationFeed;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Torrentinator.Library.Types;

namespace Torrentinator.Library.RSS
{
    public class TorrentRSSItem : ISyndicationItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IEnumerable<ISyndicationCategory> Categories { get; set; }

        public IEnumerable<ISyndicationPerson> Contributors { get; set; }

        public IEnumerable<ISyndicationLink> Links { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public DateTimeOffset Published { get; set; }
        
        public long Length { get; set; }
        public string Hash { get; set; }
        public string Magnet { get; set; }
    }
}
