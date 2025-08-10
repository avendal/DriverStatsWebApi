namespace DriverStatsWebApi.Models
{
    public class NeuerUserRequest
    {
        public string? Benutzername { get; set; }
        public string? Password { get; set; }
        public bool IstAdmin { get; set; }
        public bool IstAktiv { get; set; }
    }
}
