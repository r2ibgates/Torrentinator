using System;
using Xunit;
using Torrentinator.Library.Infrastructure;
using Torrentinator.Library.Models;

namespace Torrentinator.Library.Tests
{
    public class MongoWhereTests
    {
        [Fact]
        public void ShouldBeFindAll()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(null);
            Assert.Equal("{}", where);
        }

        [Fact]
        public void ShouldBeFindByTorrentIdExactValue()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.TorrentId == "http://thepiratebay.org/torrent/24281384/");
            Assert.Equal("{TorrentId : 'http:\\/\\/thepiratebay.org\\/torrent\\/24281384\\/'}", where);
        }

        [Fact]
        public void ShouldBeFindTorrentsThatStartWith()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.TorrentId.StartsWith("http://thepiratebay"));
            Assert.Equal("{TorrentId : /^http:\\/\\/thepiratebay/gi}", where);
        }

        [Fact]
        public void ShouldBeFindTorrentsThatEndsWith()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.TorrentId.EndsWith("1384/"));
            Assert.Equal("{TorrentId : /1384\\/$/gi}", where);
        }

        [Fact]
        public void ShouldBeFindTorrentsThatContains()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.Description.Contains("Avengers"));
            Assert.Equal("{Description : /Avengers/gi}", where);
        }

        [Fact]
        public void ShouldBeFindTorrentsByTitleAndDescription()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.Title.Contains("Avengers") && t.Description.Contains("Avengers"));
            Assert.Equal("{$and:[{Title : /Avengers/gi}, {Description : /Avengers/gi}]}", where);
        }

        [Fact]
        public void ShouldBeFindTorrentsByDescriptionAndOrTitles()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.Description.Contains("1080p") & (t.Title.Contains("Avengers") || t.Title.Contains("Jurassic")));
            Assert.Equal("{$and:[{Description : /1080p/gi}, {$or:[{Title : /Avengers/gi}, {Title : /Jurassic/gi}]}]}", where);
        }

        [Fact]
        public void ShouldBeFindTorrentsThatDoesNotEqual()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.TorrentId != "sometest");
            Assert.Equal("{TorrentId : {$ne : 'sometest'}}", where);
        }

        [Fact]
        public void ShouldBeFindTorrentsWithNullDescription()
        {
            var where = MongoWhereBuilder.ToMongoSql<Torrent>(t => t.Description == null);
            Assert.Equal("{Description : null}", where);
        }
    }
}
