using Microsoft.SyndicationFeed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Torrentinator.Models;

namespace Torrentinator.RSS
{
    public class TorrentRSSParser : ISyndicationFeedParser
    {
        public ISyndicationCategory ParseCategory(string value)
        {
            throw new NotImplementedException();
        }

        public ISyndicationContent ParseContent(string value)
        {
            throw new NotImplementedException();
        }

        public ISyndicationImage ParseImage(string value)
        {
            throw new NotImplementedException();
        }

        public ISyndicationItem ParseItem(string value)
        {
            var xml = XDocument.Parse(value);
            var nsTorrent = (XNamespace)"http://xmlns.ezrss.it/0.1/";
            return new TorrentRSSItem()
            {
                Id = xml.Descendants("guid").First().Value,
                Title = xml.Descendants("title").First().Value,
                Description = xml.Descendants("comments").First().Value,
                Categories = xml.Descendants("category").Select(n => new SyndicationCategory(n.Value)),
                Contributors = null,
                Links = xml.Descendants("link").Select(n => new SyndicationLink(new Uri(n.Value))),
                LastUpdated = DateTimeOffset.MinValue,
                Published = DateTimeOffset.Parse(xml.Descendants("pubDate").First().Value),
                Length = long.Parse(xml.Descendants(nsTorrent + "torrent").First().Descendants(nsTorrent + "contentLength").First().Value),
                Hash = xml.Descendants(nsTorrent + "torrent").First().Descendants(nsTorrent + "infoHash").First().Value,
                Magnet = xml.Descendants(nsTorrent + "torrent").First().Descendants(nsTorrent + "magnetURI").First().Value
            };
        }

        public ISyndicationLink ParseLink(string value)
        {
            throw new NotImplementedException();
        }

        public ISyndicationPerson ParsePerson(string value)
        {
            throw new NotImplementedException();
        }

        public bool TryParseValue<T>(string value, out T result)
        {
            if (typeof(T) == typeof(TorrentLink))
            {
                var ser = new XmlSerializer(typeof(TorrentLink));
                var ret = (TorrentLink)null;

                using (var sr = new StringReader(value))
                    ret = (TorrentLink)ser.Deserialize(sr);

                if (ret != null)
                {
                    result = (T)Convert.ChangeType(ret, typeof(T));
                    return true;
                }
            }

            result = default(T);
            return false;
        }
    }
}
