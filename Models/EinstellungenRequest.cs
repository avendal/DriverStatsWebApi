namespace DriverStatsWebApi.Models
{
    public class EinstellungenRequest
    {
        public decimal? Stundenlohn { get; set; }
        public bool IstMinijobber { get; set; }
        public string? Id { get; set; }
        public string? BenutzerId { get; set; }
        public string? VollständigerName { get; set; }
    }
}
