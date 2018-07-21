using Microsoft.SyndicationFeed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Torrentinator.Models
{
    [XmlRoot("item")]
    public class TorrentLink
    {        
        public string ID { get; set; }
        [XmlElement("title")]
        public string Name { get; set; }
        [XmlElement("guid")]
        public string TorrentId { get; set; }
        [XmlElement("pubDate")]
        public DateTimeOffset PublishedDate { get; set; }
        [XmlElement("link")]
        public string TorrentURL { get; set; }

        public TorrentLink() { }
        internal TorrentLink(ISyndicationItem item)
        {
            this.Name = item.Title;
            this.TorrentId = item.Description;
            this.PublishedDate = item.Published;
            this.TorrentURL = item.Links.FirstOrDefault()?.Uri?.ToString();            
        }
    }
}
