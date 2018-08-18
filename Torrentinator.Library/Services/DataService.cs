using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.Models;

namespace Torrentinator.Library.Services
{
    internal class DataService : IDataService
    {
        public class DataServiceOptions
        {
            public string ConnectionString { get; set; }
        }
        private DataServiceOptions Config { get; }
        IMongoCollection<TorrentData> collection;

        public DataService(DataServiceOptions options)
        {
            this.Config = options;
            var client = new MongoClient(this.Config.ConnectionString);            
            var db = client.GetDatabase("torrentinator");
            collection = db.GetCollection<TorrentData>("torrents");
        }

        public async Task<IEnumerable<Torrent>> GetTorrents()
        {
            var filter = new BsonDocument();

            return await collection.Find(filter).ToListAsync();
        }

        public async Task AddTorrent(Torrent torrent)
        {
            var found = collection.Find(f => f.TorrentId == torrent.TorrentId).Limit(1).Any();
            if (!found)
            {
                var td = new TorrentData(torrent);
                await collection.InsertOneAsync(td);
            }
        }

        public async Task<IEnumerable<Torrent>> GetTorrentsToAdd(IEnumerable<Torrent> torrents)
        {
            return await Task.Run(() => torrents.Where(t => !collection.Find(f => f.TorrentId == t.TorrentId).Limit(1).Any()));
        }

        public async Task AddTorrents(IEnumerable<Torrent> torrents)
        {
            var newTorrents = torrents.Where(t => !collection.Find(f => f.TorrentId == t.TorrentId).Limit(1).Any());
            if (newTorrents.Any())
            {
                var newTDs = newTorrents.Select(t => new TorrentData(t));
                await collection.InsertManyAsync(newTDs);
            }
        }

        public async Task DeleteTorrent(string id)
        {
            await collection.DeleteOneAsync(t => t.TorrentId == id);
        }
        public async Task<long> DeleteAllTorrents()
        {
            var result = await collection.DeleteManyAsync(new BsonDocument());
            return result.DeletedCount;
        }
    }
}
