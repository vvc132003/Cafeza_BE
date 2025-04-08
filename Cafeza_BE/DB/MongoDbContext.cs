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
        public IMongoCollection<Category> Categorys => _database.GetCollection<Category>("Categorys");
        public IMongoCollection<Table> Tables => _database.GetCollection<Table>("Tables");
        public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
        public IMongoCollection<Employee> Employees => _database.GetCollection<Employee>("Employees");
        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");

    }
}
