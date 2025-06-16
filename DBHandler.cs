using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace LockBox
{
    internal class DBHandler
    {
        private static DBHandler? _instance;
        private static readonly object _lock = new object();

        private const string connectionUri = "mongodb://application_user:jnZOK9k68pvgMyRuLx1bqJA25dfwtXIS@lockbox-cluster-shard-00-00.ch64s.mongodb.net:27017,lockbox-cluster-shard-00-01.ch64s.mongodb.net:27017,lockbox-cluster-shard-00-02.ch64s.mongodb.net:27017/?ssl=true&replicaSet=atlas-r51ijg-shard-0&authSource=admin&retryWrites=true&w=majority&appName=Lockbox-Cluster";
        private MongoClient _client = null!;
        private static string _dbName = "Lockbox";

        public DBHandler()
        {
            InitializeClient();
            Settings = new object();
        }

        private void InitializeClient()
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(connectionUri);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                settings.ConnectTimeout = TimeSpan.FromSeconds(30);

                settings.ClusterConfigurator = builder =>
                {
                    builder.Subscribe<CommandStartedEvent>(e =>
                    {
                        Console.WriteLine($"MongoDB Command Started: {e.CommandName} - {e.Command.ToJson()}");
                    });
                };

                // Create a new client and connect to the server
                _client = new MongoClient(settings);
            }
            catch (Exception e)
            {
                _ = Application.Current?.Windows[0].Page?.DisplayAlert("Error", e.Message + e.StackTrace, "Ok");
            }
        }

        public static DBHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DBHandler();
                        }
                    }
                }
                return _instance;
            }
        }

        public object Settings { get; set; }

        public async Task CreateEntryAsync(string collectionName, ServiceEntry entry)
        {
            EnsureClientInitialized();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<ServiceEntry>(collectionName);
            await collection.InsertOneAsync(entry);
        }

        public async Task<int> GetEntryCountAsync(string collectionName)
        {
            EnsureClientInitialized();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<ServiceEntry>(collectionName);
            return (int)await collection.CountDocumentsAsync(FilterDefinition<ServiceEntry>.Empty);
        }

        public List<ServiceEntry> online = new List<ServiceEntry>();
        public async Task<List<ServiceEntry>> GetAllEntriesAsync(string collectionName)
        {
            EnsureClientInitialized();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<ServiceEntry>(collectionName);
            return await collection.Find(FilterDefinition<ServiceEntry>.Empty).ToListAsync();
        }

        public async Task UpdateEntryAsync(int dbId, string collectionName, ServiceEntry updatedEntry)
        {
            EnsureClientInitialized();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<ServiceEntry>(collectionName);
            var filter = Builders<ServiceEntry>.Filter.Eq(e => e.Db_id, dbId);
            await collection.ReplaceOneAsync(filter, updatedEntry);
        }

        public async Task DeleteEntryAsync(int dbId, string collectionName)
        {
            EnsureClientInitialized();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<ServiceEntry>(collectionName);
            var filter = Builders<ServiceEntry>.Filter.Eq(e => e.Db_id, dbId);
            await collection.DeleteOneAsync(filter);
        }

        public async Task<bool> IsCollectionEmptyAsync(string collectionName)
        {
            EnsureClientInitialized();

            var database = _client.GetDatabase(_dbName);
            var collection = database.GetCollection<ServiceEntry>(collectionName);
            var count = await collection.CountDocumentsAsync(FilterDefinition<ServiceEntry>.Empty);
            return count == 0;
        }

        private void EnsureClientInitialized()
        {
            if (_client == null)
            {
                InitializeClient();
            }
        }
    }
}
