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
        //public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("Customers");
        //public IMongoCollection<Employee> Employees => _database.GetCollection<Employee>("Employees");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<CustomerDetails> CustomerDetails => _database.GetCollection<CustomerDetails>("CustomerDetails");
        public IMongoCollection<EmployeeDetails> EmployeeDetails => _database.GetCollection<EmployeeDetails>("EmployeeDetails");
        public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
        public IMongoCollection<OrderDetail> OrderDetails => _database.GetCollection<OrderDetail>("OrderDetails");
        public IMongoCollection<OrderCancellation> OrderCancellations => _database.GetCollection<OrderCancellation>("OrderCancellations");
        public IMongoCollection<TableTransfer> TableTransfers => _database.GetCollection<TableTransfer>("TableTransfers");
        public IMongoCollection<Conversation> Conversations => _database.GetCollection<Conversation>("Conversations");
        public IMongoCollection<ConversationMembers> ConversationMembers => _database.GetCollection<ConversationMembers>("ConversationMembers");
        public IMongoCollection<Message> Messages => _database.GetCollection<Message>("Messages");


    }
}
