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
    public class DataService : IDataService
    {
        public class DataServiceOptions
        {
            public string ConnectionString { get; set; }
        }
        private DataServiceOptions Config { get; }
        IMongoCollection<Torrent> collection;

        public DataService(DataServiceOptions options)
        {
            this.Config = options;
            var client = new MongoClient();
            var db = client.GetDatabase("torrentinator");
            collection = db.GetCollection<Torrent>("torrents");
        }

        public async Task<IEnumerable<Torrent>> GetTorrents()
        {
            /*
            var ret = new List<Torrent>()
            {
                new Torrent()
                {
                    Id = "test-1",
                    Title = this.Config.ConnectionString,
                    Description = "This is only a test... duh",
                    Length = 492020,
                    Published = new DateTimeOffset(2018, 03, 15, 12, 4,6,TimeSpan.FromHours(0)),
                    Url = "https://www.google.com"
                }
            };

            return await Task.FromResult<IEnumerable<Torrent>>(ret);
            */
            var filter = new BsonDocument();

            return await collection.Find(filter).ToListAsync();
        }

        public async Task RefreshTorrents()
        {
            var found = await collection.FindAsync(f => f.Id == "test-123");
            if (!found.Any())
            {
                await collection.InsertOneAsync(new Torrent()
                {
                    Id = "test-123",
                    Title = "From the DB",
                    Description = "yup, just a dumb test",
                    Length = 492020,
                    Published = new DateTimeOffset(2018, 03, 15, 12, 4, 6, TimeSpan.FromHours(0)),
                    Url = "https://www.google.com"
                });
            }
        }
    }
}
