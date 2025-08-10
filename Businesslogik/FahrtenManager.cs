using DriverStatsWebApi.Models;

namespace DriverStatsWebApi.Businesslogik
{
    public class FahrtenManager
    {
        public FahrtDaten VerarbeiteFahrt(FahrtSpeichernRequest fahrt)
        {
            // Berechne GefahreneZeit (TimeSpan) unter Berücksichtigung eines möglichen Tagwechsels
            var format = "HH:mm";
            var beginn = TimeSpan.TryParse(fahrt.Beginn, out var parsedBeginn) ? parsedBeginn : TimeSpan.Zero;
            if (parsedBeginn == TimeSpan.Zero)
            {
                throw new ArgumentException("Ungültiges Zeitformat für Beginn: " + fahrt.Beginn);
            }
            var ende = TimeSpan.TryParse(fahrt.Ende, out var parsedEnde) ? parsedEnde : TimeSpan.Zero;
            if (parsedEnde == TimeSpan.Zero)
            {
                throw new ArgumentException("Ungültiges Zeitformat für Ende: " + fahrt.Ende);
            }
            TimeSpan gefahreneZeit;
            if (parsedEnde >= parsedBeginn)
            {
                gefahreneZeit = parsedEnde - parsedBeginn;
            }
            else
            {
                // Tagwechsel: Ende ist am nächsten Tag
                // Zeit von Beginn bis Mitternacht plus Zeit von Mitternacht bis Ende
                gefahreneZeit = (TimeSpan.FromHours(24) - parsedBeginn) + parsedEnde;
            }

            var gefahreneZeitDezimal = Math.Round(gefahreneZeit.TotalHours, 2);
            // Stundenlohn als decimal parsen
            decimal stundenlohnDecimal = 0.0m;
            decimal.TryParse(fahrt.Stundenlohn, out stundenlohnDecimal);
            // BetragTag berechnen
            decimal betragTag = Math.Round((decimal)gefahreneZeitDezimal * stundenlohnDecimal, 2);

            var daten = new FahrtDaten
            {
                // Übernehme alle Werte aus fahrt (durch Vererbung)
                Datum = fahrt.Datum,
                Beginn = fahrt.Beginn,
                Ende = fahrt.Ende,
                GefahreneKm = fahrt.GefahreneKm,
                Stundenlohn = fahrt.Stundenlohn,
                GefahreneZeit = gefahreneZeit,
                BetragTag = betragTag,
                Benutzername = fahrt.Benutzername,
                Id = fahrt.Id, // Behalte die ID bei
                GefahreneZeitDezimal = gefahreneZeitDezimal // Gefahrene Zeit als Dezimalwert mit 2 Nachkommastellen
            };
            return daten;
        }
    }
}
