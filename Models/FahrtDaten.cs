using System;
using DriverStatsWebApi.Models;

namespace DriverStatsWebApi.Models
{
    public class FahrtDaten : FahrtSpeichernRequest
    {
        public TimeSpan GefahreneZeit { get; set; }
        public decimal BetragTag { get; set; }
        public double GefahreneZeitDezimal { get; set; }
        
    }
}
