using Microsoft.AspNetCore.Mvc;
using DriverStatsWebApi.Models;
using Repository;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace DriverStatsWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string? _mongoConnectionString;
        private readonly string? _mongoDatabaseName;
        private readonly string? _mongoUserCollection;

        public AuthController(IConfiguration configuration)
        {
            _mongoConnectionString = configuration["MongoDb:ConnectionString"];
            _mongoDatabaseName = configuration["MongoDb:DatabaseName"];
            _mongoUserCollection = configuration["MongoDb:UserCollection"];
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(_mongoConnectionString) || string.IsNullOrWhiteSpace(_mongoDatabaseName) || string.IsNullOrWhiteSpace(_mongoUserCollection))
            {
                return StatusCode(500, new { success = false, message = "MongoDB-Konfiguration fehlt oder ist ungültig." });
            }

            var repo = new UserCollectionRepository(
                _mongoConnectionString,
                _mongoDatabaseName,
                _mongoUserCollection
            );
            var userDoc = repo.GetUser(request.Username);
            if (userDoc.Contains("error"))
            {
                return Unauthorized(new { success = false, message = "Ungültiger Benutzer" });
            }
            var dbPassword = userDoc.GetValue("Password", "").AsString;
            if (dbPassword != request.Password)
            {
                return Unauthorized(new { success = false, message = "Falsches Passwort" });
            }
            var istAdmin = userDoc.GetValue("IstAdmin", false).ToBoolean();
            return Ok(new { success = true, message = "Login erfolgreich", istAdmin });
        }

        [HttpPost("neuerUser")]
        public IActionResult NeuerUser([FromBody] NeuerUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(_mongoConnectionString) || string.IsNullOrWhiteSpace(_mongoDatabaseName) || string.IsNullOrWhiteSpace(_mongoUserCollection))
            {
                return StatusCode(500, new { success = false, message = "MongoDB-Konfiguration fehlt oder ist ungültig." });
            }
            try
            {
                var repo = new UserCollectionRepository(
                    _mongoConnectionString,
                    _mongoDatabaseName,
                    _mongoUserCollection
                );
                repo.SaveUser(request);
                return Ok(new
                {
                    success = true,
                    message = "Neuer Benutzer gespeichert",
                    benutzer = request.Benutzername,
                    istAdmin = request.IstAdmin
                });
            }
            catch (MongoException ex)
            {
                return StatusCode(500, new { success = false, message = "Fehler beim Speichern des Benutzers", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Unbekannter Fehler beim Speichern des Benutzers", error = ex.Message });
            }
        }

        [HttpGet("userlist")]
        public IActionResult GetUserList()
        {
            if (string.IsNullOrWhiteSpace(_mongoConnectionString) || string.IsNullOrWhiteSpace(_mongoDatabaseName) || string.IsNullOrWhiteSpace(_mongoUserCollection))
            {
                return StatusCode(500, new { success = false, message = "MongoDB-Konfiguration fehlt oder ist ungültig." });
            }
            try
            {
                var repo = new UserCollectionRepository(
                    _mongoConnectionString,
                    _mongoDatabaseName,
                    _mongoUserCollection
                );
                var userDocs = repo.GetUserList();
                var userList = userDocs.Select(doc => new NeuerUserRequest
                {
                    Benutzername = doc.GetValue("Benutzername", "").AsString,
                    Password = doc.GetValue("Password", "").AsString,
                    IstAdmin = doc.GetValue("IstAdmin", false).ToBoolean()
                }).ToList();
                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fehler beim Abrufen der Benutzerliste", error = ex.Message });
            }
        }
    }
}
