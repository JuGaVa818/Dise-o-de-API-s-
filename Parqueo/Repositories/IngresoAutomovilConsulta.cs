namespace Parqueo.Repositories;

public sealed class IngresoAutomovilConsulta(
    int consecutivo,
    Automovil automovil,
    DateTime fechaHoraEntrada,
    DateTime? fechaHoraSalida,
    decimal? precioPorHora)
{
    public int Consecutivo { get; } = consecutivo;
    public Automovil Automovil { get; } = automovil;
    public DateTime FechaHoraEntrada { get; } = fechaHoraEntrada;
    public DateTime? FechaHoraSalida { get; } = fechaHoraSalida;
    public decimal? PrecioPorHora { get; } = precioPorHora;

    public decimal? DuracionEstadiaMinutos => FechaHoraSalida is { } salida
        ? (decimal)(salida.Ticks - FechaHoraEntrada.Ticks) / TimeSpan.TicksPerMinute
        : null;

    public decimal? DuracionEstadiaHoras => FechaHoraSalida is { } salida
        ? (decimal)(salida.Ticks - FechaHoraEntrada.Ticks) / TimeSpan.TicksPerHour
        : null;

    public decimal? MontoTotalPagar => DuracionEstadiaHoras is { } horas &&
                                       PrecioPorHora is { } precio
        ? horas * precio
        : null;
}
