using DriverStatsWebApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository
{
    public class UserCollectionRepository : MongoDbBase
    {
        private readonly IMongoCollection<BsonDocument> _userCollection;

        public UserCollectionRepository(string connectionString, string databaseName, string collectionName)
            : base(connectionString, databaseName)
        {
            _userCollection = Database.GetCollection<BsonDocument>(collectionName);
        }

        /// <summary>
        /// Speichert oder aktualisiert einen Benutzer in der User-Collection.
        /// </summary>
        /// <param name="user">Das Benutzerobjekt vom Typ NeuerUserRequest.</param>
        public void SaveUser(NeuerUserRequest user)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Benutzername", user.Benutzername);
            var update = Builders<BsonDocument>.Update
                .Set("Benutzername", user.Benutzername)
                .Set("Password", user.Password)
                .Set("IstAdmin", user.IstAdmin);

            var options = new UpdateOptions { IsUpsert = true };
            _userCollection.UpdateOne(filter, update, options);
        }

        /// <summary>
        /// Sucht einen Benutzer anhand des Benutzernamens.
        /// </summary>
        /// <param name="benutzername">Der Benutzername.</param>
        /// <returns>Das Benutzerobjekt als BsonDocument oder eine Fehlermeldung.</returns>
        public BsonDocument GetUser(string benutzername)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Benutzername", benutzername);
            var user = _userCollection.Find(filter).FirstOrDefault();
            if (user == null)
            {
                var error = new BsonDocument
                {
                    { "error", "Ungültiger Benutzer" }
                };
                return error;
            }
            return user;
        }

        /// <summary>
        /// Gibt eine Liste aller Benutzer in der User-Collection zurück.
        /// </summary>
        /// <returns>Liste aller Benutzer als BsonDocument.</returns>
        public List<BsonDocument> GetUserList()
        {
            try
            {
                return _userCollection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der Benutzerliste aus der Datenbank", ex);
            }
        }
    }
}
