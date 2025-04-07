using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace Cafeza_BE.DB
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Drink> Drinks => _database.GetCollection<Drink>("Drinks");

    }
}
