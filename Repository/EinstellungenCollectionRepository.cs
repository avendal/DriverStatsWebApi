using DriverStatsWebApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository
{
    public class EinstellungenCollectionRepository : MongoDbBase
    {
        private readonly IMongoCollection<BsonDocument> _einstellungenCollection;

        public EinstellungenCollectionRepository(string connectionString, string databaseName, string collectionName)
            : base(connectionString, databaseName)
        {
            _einstellungenCollection = Database.GetCollection<BsonDocument>(collectionName);
        }

        /// <summary>
        /// Speichert oder aktualisiert Einstellungen in der Datenbank.
        /// </summary>
        /// <param name="einstellungen">Das EinstellungenRequest-Objekt.</param>
        public void SaveEinstellungen(EinstellungenRequest einstellungen)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("BenutzerId", einstellungen.BenutzerId);
            var update = Builders<BsonDocument>.Update
                .Set("Stundenlohn", einstellungen.Stundenlohn.HasValue ? Math.Round(einstellungen.Stundenlohn.Value, 2) : (decimal?)null)
                .Set("IstMinijobber", einstellungen.IstMinijobber)
                .Set("BenutzerId", einstellungen.BenutzerId)
                .Set("VollständigerName", einstellungen.VollständigerName);

            var options = new UpdateOptions { IsUpsert = true };
            _einstellungenCollection.UpdateOne(filter, update, options);
        }

        /// <summary>
        /// Lädt die Einstellungen für einen bestimmten Benutzer aus der Datenbank.
        /// </summary>
        /// <param name="benutzerId">Die Benutzer-ID.</param>
        /// <returns>Das EinstellungenRequest-Objekt oder null, wenn nichts gefunden wurde.</returns>
        public EinstellungenRequest? GetEinstellungen(string benutzerId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("BenutzerId", benutzerId);
            var doc = _einstellungenCollection.Find(filter).FirstOrDefault();
            if (doc == null)
                return null;

            return new EinstellungenRequest
            {
                Stundenlohn = doc.Contains("Stundenlohn") ? doc["Stundenlohn"].ToDecimal() : null,
                IstMinijobber = doc.GetValue("IstMinijobber", false).ToBoolean(),
                BenutzerId = doc.GetValue("BenutzerId", null)?.AsString,
                VollständigerName = doc.GetValue("VollständigerName", null)?.AsString,
                Id = doc.Contains("_id") ? doc["_id"].ToString() : null
            };
        }
    }
}
