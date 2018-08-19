using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Torrentinator.Library.Models;
using Torrentinator.Library.Infrastructure;
using System.Linq.Expressions;

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

        public async Task<IEnumerable<Torrent>> GetTorrents(Expression<Func<Torrent, bool>> filter)
        {
            if (filter != null)
            {
                var whereString = MongoWhereBuilder.ToMongoSql(filter);
               
                return await collection.Find(whereString).ToListAsync();
            }
            else
                return await collection.Find(new BsonDocument()).ToListAsync();
        }
        public async Task<Torrent> GetTorrent(string id)
        {
            var options = new FindOptions<TorrentData>
            {
                Limit = 1,                
                NoCursorTimeout = false
            };
            return await collection.FindAsync(t => t.TorrentId == id, options).Result.FirstOrDefaultAsync();
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
        
        public async Task<long> DeleteTorrents(Expression<Func<Torrent, bool>> filter)
        {
            var result = await collection.DeleteManyAsync(MongoWhereBuilder.ToMongoSql(filter));
            return result.DeletedCount;
        }

        public async Task<Torrent> UpdateTorrent(Torrent torrent)
        {
            var result = await collection.ReplaceOneAsync(
                   Builders<TorrentData>.Filter.Eq(t => t.TorrentId, torrent.TorrentId),
                   new TorrentData(torrent), new UpdateOptions() { IsUpsert = true });
            if (result.IsAcknowledged)
                return torrent;
            else
                return null;
        }
    }
}
