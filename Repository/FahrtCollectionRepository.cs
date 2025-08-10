using DriverStatsWebApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repository
{
    public class FahrtCollectionRepository : MongoDbBase
    {
        private readonly IMongoCollection<BsonDocument> _fahrtCollection;

        public FahrtCollectionRepository(string connectionString, string databaseName, string collectionName)
            : base(connectionString, databaseName)
        {
            _fahrtCollection = Database.GetCollection<BsonDocument>(collectionName);
        }

        /// <summary>
        /// Speichert oder aktualisiert eine Fahrt in der Fahrt-Collection.
        /// Wenn sich Datum, Beginn oder Ende ändern, werden die alten Werte als Suchkriterium verwendet.
        /// </summary>
        /// <param name="fahrt">Das Fahrtobjekt vom Typ FahrDaten.</param>
        /// <param name="altesDatum">Optional: Altes Datum zur Identifikation.</param>
        /// <param name="alterBeginn">Optional: Alter Beginn zur Identifikation.</param>
        /// <param name="altesEnde">Optional: Altes Ende zur Identifikation.</param>
        /// <param name="alterBenutzername">Optional: Alter Benutzername zur Identifikation.</param>
        public void SaveFahrt(FahrtDaten fahrt)
        {
            // Suche nach alten Werten, falls vorhanden, sonst nach aktuellen Werten
            // Wenn Id vorhanden ist, nach _id filtern, sonst nach den anderen Feldern
            FilterDefinition<BsonDocument> filter;
            if (!string.IsNullOrEmpty(fahrt.Id))
            {
                filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(fahrt.Id));
            }
            else
            {
                filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("Datum", fahrt.Datum),
                    Builders<BsonDocument>.Filter.Eq("Beginn", fahrt.Beginn),
                    Builders<BsonDocument>.Filter.Eq("Ende", fahrt.Ende),
                    Builders<BsonDocument>.Filter.Eq("Benutzername", fahrt.Benutzername)
                );
            }

            var update = Builders<BsonDocument>.Update
                .Set("Datum", fahrt.Datum)
                .Set("Beginn", fahrt.Beginn)
                .Set("Ende", fahrt.Ende)
                .Set("GefahreneKm", fahrt.GefahreneKm)
                .Set("Stundenlohn", fahrt.Stundenlohn)
                .Set("Benutzername", fahrt.Benutzername)
                .Set("GefahreneZeit", fahrt.GefahreneZeit.ToString())
                .Set("GefahreneZeitDezimal", fahrt.GefahreneZeitDezimal)
                .Set("BetragTag", fahrt.BetragTag);

            var options = new UpdateOptions { IsUpsert = true };
            _fahrtCollection.UpdateOne(filter, update, options);
        }

        /// <summary>
        /// Gibt alle Fahrten eines Benutzers zurück.
        /// </summary>
        /// <param name="benutzername">Der Benutzername.</param>
        /// <returns>Liste der Fahrten als BsonDocument.</returns>
        public List<FahrtDaten> GetFahrtenByBenutzer(string benutzername)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Benutzername", benutzername);
            var bsonList = _fahrtCollection.Find(filter).ToList();
            var result = new List<FahrtDaten>();
            var fahrtenManager = new DriverStatsWebApi.Businesslogik.FahrtenManager();
            foreach (var doc in bsonList)
            {
                var mapped = MappeBsonZuFahrtDaten(doc);
                // Umwandlung in FahrtSpeichernRequest für VerarbeiteFahrt
                var request = new DriverStatsWebApi.Models.FahrtSpeichernRequest
                {
                    Datum = mapped.Datum,
                    Beginn = mapped.Beginn,
                    Ende = mapped.Ende,
                    GefahreneKm = mapped.GefahreneKm,
                    Stundenlohn = mapped.Stundenlohn,
                    Benutzername = mapped.Benutzername,
                };
                var processed = fahrtenManager.VerarbeiteFahrt(request);

                processed.Id = mapped.Id; // Behalte die ID bei

                result.Add(processed);
            }
            return result;
        }

        /// <summary>
        /// Löscht eine Fahrt anhand der eindeutigen Felder Datum, Beginn, Ende und Benutzername.
        /// </summary>
        /// <param name="request">Das FahrtSpeichernRequest-Objekt mit den zu löschenden Fahrtdaten.</param>
        /// <returns>True, wenn ein Dokument gelöscht wurde, sonst false.</returns>
        public bool DeleteFahrt(FahrtSpeichernRequest request)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("Datum", request.Datum),
                Builders<BsonDocument>.Filter.Eq("Beginn", request.Beginn),
                Builders<BsonDocument>.Filter.Eq("Ende", request.Ende),
                Builders<BsonDocument>.Filter.Eq("Benutzername", request.Benutzername)
            );
            var result = _fahrtCollection.DeleteOne(filter);
            return result.DeletedCount > 0;
        }

        private FahrtDaten MappeBsonZuFahrtDaten(BsonDocument doc)
        {
            var fahrt = new FahrtDaten
            {
                Datum = doc.GetValue("Datum", "").AsString,
                Beginn = doc.GetValue("Beginn", "").AsString,
                Ende = doc.GetValue("Ende", "").AsString,
                GefahreneKm = doc.GetValue("GefahreneKm", "").AsString,
                Stundenlohn = doc.GetValue("Stundenlohn", "").AsString,
                Benutzername = doc.GetValue("Benutzername", "").AsString,
                GefahreneZeitDezimal = doc.GetValue("GefahreneZeitDezimal", 0.0).ToDouble(),
                BetragTag = doc.GetValue("BetragTag", 0.0m).ToDecimal()
            };

            // GefahreneZeit exakt wie in der Datenbank gespeichert übernehmen
            if (doc.Contains("GefahreneZeit"))
            {
                var raw = doc.GetValue("GefahreneZeit");
                if (raw.IsString)
                {
                    TimeSpan.TryParse(raw.AsString, out var ts);
                    fahrt.GefahreneZeit = ts;
                }
                else if (raw.IsDouble || raw.IsInt32 || raw.IsInt64)
                {
                    fahrt.GefahreneZeit = TimeSpan.FromHours(raw.ToDouble());
                }
                else if (raw.IsBsonNull)
                {
                    fahrt.GefahreneZeit = TimeSpan.Zero;
                }
            }

            // _id auslesen, falls vorhanden
            if (doc.Contains("_id"))
            {
                fahrt.Id = doc["_id"].ToString();
            }

            return fahrt;
        }
    }
}
