using System.Text.Json.Serialization;

namespace Parqueo.Controllers;

public sealed class IngresoAutomovilResponse
{
    [JsonPropertyName("consecutivo")]
    public int Consecutivo { get; init; }

    [JsonPropertyName("id_parqueo")]
    public int IdParqueo { get; init; }

    [JsonPropertyName("id_automovil")]
    public int IdAutomovil { get; init; }

    [JsonPropertyName("fecha_hora_entrada")]
    public DateTime FechaHoraEntrada { get; init; }

    [JsonPropertyName("fecha_hora_salida")]
    public DateTime? FechaHoraSalida { get; init; }

    [JsonPropertyName("parqueo")]
    public Parqueo Parqueo { get; init; } = null!;

    [JsonPropertyName("automovil")]
    public Automovil Automovil { get; init; } = null!;

    [JsonPropertyName("duracion_estadia_minutos")]
    public decimal? DuracionEstadiaMinutos { get; init; }

    [JsonPropertyName("duracion_estadia_horas")]
    public decimal? DuracionEstadiaHoras { get; init; }

    [JsonPropertyName("monto_total_pagar")]
    public decimal? MontoTotalPagar { get; init; }

    public static IngresoAutomovilResponse FromEntity(IngresoAutomovil ingreso)
    {
        var parqueo = ingreso.Parqueo
            ?? throw new InvalidOperationException(
                $"El ingreso {ingreso.Consecutivo} no tiene parqueo relacionado.");
        var automovil = ingreso.Automovil
            ?? throw new InvalidOperationException(
                $"El ingreso {ingreso.Consecutivo} no tiene automóvil relacionado.");

        return new IngresoAutomovilResponse
        {
            Consecutivo = ingreso.Consecutivo,
            IdParqueo = ingreso.IdParqueo,
            IdAutomovil = ingreso.IdAutomovil,
            FechaHoraEntrada = ingreso.FechaHoraEntrada,
            FechaHoraSalida = ingreso.FechaHoraSalida,
            Parqueo = parqueo,
            Automovil = automovil,
            DuracionEstadiaMinutos = ingreso.DuracionEstadiaMinutos,
            DuracionEstadiaHoras = ingreso.DuracionEstadiaHoras,
            MontoTotalPagar = ingreso.MontoTotalPagar
        };
    }
}
