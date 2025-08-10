namespace DriverStatsWebApi.Models
{
    public class FahrtSpeichernRequest
    {
        public string Datum { get; set; } = string.Empty;
        public string Beginn { get; set; } = string.Empty;
        public string Ende { get; set; } = string.Empty;
        public string GefahreneKm { get; set; } = string.Empty;
        public string Stundenlohn { get; set; } = string.Empty;
        public string Benutzername { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }
}
