using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Parqueo;

public sealed class IngresoAutomovil
{
    [JsonPropertyName("consecutivo")]
    public int Consecutivo { get; set; }

    [JsonPropertyName("id_parqueo")]
    public int IdParqueo { get; set; }

    [JsonPropertyName("id_automovil")]
    public int IdAutomovil { get; set; }

    [JsonPropertyName("fecha_hora_entrada")]
    public DateTime FechaHoraEntrada { get; set; }

    [JsonPropertyName("fecha_hora_salida")]
    public DateTime? FechaHoraSalida { get; set; }

    [JsonIgnore]
    public Parqueo? Parqueo { get; set; }

    [JsonIgnore]
    public Automovil? Automovil { get; set; }

    [NotMapped]
    [JsonIgnore]
    public decimal? DuracionEstadiaMinutos => FechaHoraSalida is { } salida
        ? (decimal)(salida.Ticks - FechaHoraEntrada.Ticks) / TimeSpan.TicksPerMinute
        : null;

    [NotMapped]
    [JsonIgnore]
    public decimal? DuracionEstadiaHoras => FechaHoraSalida is { } salida
        ? (decimal)(salida.Ticks - FechaHoraEntrada.Ticks) / TimeSpan.TicksPerHour
        : null;

    [NotMapped]
    [JsonIgnore]
    public decimal? MontoTotalPagar => DuracionEstadiaHoras is { } horas &&
                                       Parqueo is not null
        ? horas * Parqueo.PrecioPorHora
        : null;
}
