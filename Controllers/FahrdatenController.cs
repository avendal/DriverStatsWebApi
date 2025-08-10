using Microsoft.AspNetCore.Mvc;
using DriverStatsWebApi.Models;

namespace DriverStatsWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FahrdatenController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _collectionName;

        public FahrdatenController(IConfiguration configuration)
        {
            _connectionString = configuration["MongoDb:ConnectionString"] ?? string.Empty;
            _databaseName = configuration["MongoDb:DatabaseName"] ?? string.Empty;
            _collectionName = configuration["MongoDb:fahrtenCollection"] ?? string.Empty;
        }

        [HttpPost("speicherFahrt")]
        public IActionResult SpeicherFahrt([FromBody] FahrtSpeichernRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Ungültige Fahrtdaten." });
            }

            // Prüfe, ob alle Felder gefüllt sind
            var fehlendeFelder = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Datum)) fehlendeFelder.Add(nameof(request.Datum));
            if (string.IsNullOrWhiteSpace(request.Beginn)) fehlendeFelder.Add(nameof(request.Beginn));
            if (string.IsNullOrWhiteSpace(request.Ende)) fehlendeFelder.Add(nameof(request.Ende));
            if (string.IsNullOrWhiteSpace(request.GefahreneKm)) fehlendeFelder.Add(nameof(request.GefahreneKm));
            if (string.IsNullOrWhiteSpace(request.Stundenlohn)) fehlendeFelder.Add(nameof(request.Stundenlohn));
            if (string.IsNullOrWhiteSpace(request.Benutzername)) fehlendeFelder.Add(nameof(request.Benutzername));

            if (fehlendeFelder.Count > 0)
            {
                return BadRequest(new { success = false, message = "Folgende Felder fehlen: ", fehlendeFelder });
            }

            var manager = new DriverStatsWebApi.Businesslogik.FahrtenManager();
            var fahrtDaten = manager.VerarbeiteFahrt(request);
            if (fahrtDaten == null)
            {
                return StatusCode(500, new { success = false, message = "Fehler bei der Verarbeitung der Fahrtdaten." });
            }

            try
            {
                var repo = new Repository.FahrtCollectionRepository(_connectionString, _databaseName, _collectionName);
                repo.SaveFahrt(fahrtDaten);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fehler beim Speichern der Fahrtdaten.", error = ex.Message });
            }

            return Ok(new { success = true, message = "Fahrt erfolgreich gespeichert.", daten = fahrtDaten });
        }

        [HttpGet("getFahrten")]
        public IActionResult GetFahrten([FromQuery] string benutzername)
        {
            if (string.IsNullOrWhiteSpace(benutzername))
            {
                return BadRequest(new { success = false, message = "Benutzername darf nicht leer sein." });
            }
            try
            {
                var repo = new Repository.FahrtCollectionRepository(_connectionString, _databaseName, _collectionName);
                var fahrtDocs = repo.GetFahrtenByBenutzer(benutzername);
                if (fahrtDocs == null || fahrtDocs.Count == 0)
                {
                    return NotFound(new { success = false, message = "Keine Fahrten für diesen Benutzer gefunden." });
                }
                return Ok(fahrtDocs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fehler beim Abrufen der Fahrten.", error = ex.Message });
            }
        }

        [HttpPost("loescheFahrt")]
        public IActionResult LoescheFahrt([FromBody] FahrtSpeichernRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Ungültige Fahrtdaten." });
            }

            // Prüfe, ob die notwendigen Felder vorhanden sind
            var fehlendeFelder = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Datum)) fehlendeFelder.Add(nameof(request.Datum));
            if (string.IsNullOrWhiteSpace(request.Beginn)) fehlendeFelder.Add(nameof(request.Beginn));
            if (string.IsNullOrWhiteSpace(request.Ende)) fehlendeFelder.Add(nameof(request.Ende));
            if (string.IsNullOrWhiteSpace(request.Benutzername)) fehlendeFelder.Add(nameof(request.Benutzername));

            if (fehlendeFelder.Count > 0)
            {
                return BadRequest(new { success = false, message = "Folgende Felder fehlen: ", fehlendeFelder });
            }

            try
            {
                var repo = new Repository.FahrtCollectionRepository(_connectionString, _databaseName, _collectionName);
                bool deleted = repo.DeleteFahrt(request);
                if (!deleted)
                {
                    return NotFound(new { success = false, message = "Fahrt nicht gefunden oder konnte nicht gelöscht werden." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fehler beim Löschen der Fahrtdaten.", error = ex.Message });
            }

            return Ok(new { success = true, message = "Fahrt erfolgreich gelöscht." });
        }

        [HttpPost("updateFahrt")]
        public IActionResult UpdateFahrt([FromBody] FahrtSpeichernRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Ungültige Fahrtdaten." });
            }

            // Prüfe, ob alle Felder gefüllt sind
            var fehlendeFelder = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Datum)) fehlendeFelder.Add(nameof(request.Datum));
            if (string.IsNullOrWhiteSpace(request.Beginn)) fehlendeFelder.Add(nameof(request.Beginn));
            if (string.IsNullOrWhiteSpace(request.Ende)) fehlendeFelder.Add(nameof(request.Ende));
            if (string.IsNullOrWhiteSpace(request.GefahreneKm)) fehlendeFelder.Add(nameof(request.GefahreneKm));
            if (string.IsNullOrWhiteSpace(request.Stundenlohn)) fehlendeFelder.Add(nameof(request.Stundenlohn));
            if (string.IsNullOrWhiteSpace(request.Benutzername)) fehlendeFelder.Add(nameof(request.Benutzername));

            if (fehlendeFelder.Count > 0)
            {
                return BadRequest(new { success = false, message = "Folgende Felder fehlen: ", fehlendeFelder });
            }

            var manager = new DriverStatsWebApi.Businesslogik.FahrtenManager();
            var fahrtDaten = manager.VerarbeiteFahrt(request);
            if (fahrtDaten == null)
            {
                return StatusCode(500, new { success = false, message = "Fehler bei der Verarbeitung der Fahrtdaten." });
            }

            try
            {
                var repo = new Repository.FahrtCollectionRepository(_connectionString, _databaseName, _collectionName);
                repo.SaveFahrt(fahrtDaten); // SaveFahrt macht Upsert (Update oder Insert)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fehler beim Aktualisieren der Fahrtdaten.", error = ex.Message });
            }

            return Ok(fahrtDaten);
        }
    }
}
