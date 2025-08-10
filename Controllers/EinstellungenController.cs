using Microsoft.AspNetCore.Mvc;
using DriverStatsWebApi.Models;
using Repository;
using Microsoft.Extensions.Configuration;

namespace DriverStatsWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EinstellungenController : ControllerBase
    {
         private readonly string? _mongoConnectionString;
        private readonly string? _mongoDatabaseName;
        private readonly string? _mongoUserCollection;
        public EinstellungenController(IConfiguration configuration)
        {
            var mongoSection = configuration.GetSection("MongoDb");
            _mongoConnectionString = mongoSection["ConnectionString"] ?? string.Empty;
            _mongoDatabaseName = mongoSection["DatabaseName"] ?? string.Empty;
            _mongoUserCollection = mongoSection["einstellungenCollection"] ?? string.Empty;
        }

        [HttpPost("speicherEinstellungen")]
        public IActionResult SpeicherEinstellungen([FromBody] EinstellungenRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Ungültige Einstellungsdaten." });
            }

            if (!request.Stundenlohn.HasValue)
            {
                return BadRequest(new { success = false, message = "Stundenlohn darf nicht leer sein." });
            }

            if (string.IsNullOrWhiteSpace(request.VollständigerName))
            {
                return BadRequest(new { success = false, message = "Vollständiger Name darf nicht leer sein." });
            }

            var einstellungenRepository = new EinstellungenCollectionRepository(_mongoConnectionString, _mongoDatabaseName, _mongoUserCollection);
            einstellungenRepository.SaveEinstellungen(request);

            return Ok(new { success = true, message = "Einstellungen erfolgreich gespeichert.", daten = request });
        }

        [HttpGet("geteinstellungen")]
        public IActionResult GetEinstellungen([FromQuery] string benutzerId)
        {
            if (string.IsNullOrWhiteSpace(benutzerId))
            {
                return BadRequest(new { success = false, message = "BenutzerId darf nicht leer sein." });
            }

            var einstellungenRepository = new EinstellungenCollectionRepository(_mongoConnectionString, _mongoDatabaseName, _mongoUserCollection);
            var einstellungen = einstellungenRepository.GetEinstellungen(benutzerId);

            if (einstellungen == null)
            {
                return NotFound(new { success = false, message = "Einstellungen für den Benutzer wurden nicht gefunden." });
            }

            return Ok(new { success = true, daten = einstellungen });
        }
    }
}
