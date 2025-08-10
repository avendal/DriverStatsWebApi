using MongoDB.Driver;

namespace Repository
{
    /// <summary>
    /// Basisklasse für die MongoDB-Verbindungsherstellung und Zugriff.
    /// </summary>
    public abstract class MongoDbBase
    {
        protected readonly IMongoDatabase Database;
        protected readonly MongoClient MongoClient;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="MongoDbBase"/> Klasse.
        /// </summary>
        /// <param name="connectionString">Die Verbindungszeichenfolge zur MongoDB.</param>
        /// <param name="databaseName">Der Name der Datenbank.</param>
        protected MongoDbBase(string connectionString, string databaseName)
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            MongoClient = new MongoClient(settings);
            Database = MongoClient.GetDatabase(databaseName);
        }
    }
}
