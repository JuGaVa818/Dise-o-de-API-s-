using System.ComponentModel.DataAnnotations;

namespace Parqueo.Controllers;

public sealed class IngresoAutomovilRequest : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int IdParqueo { get; init; }

    [Range(1, int.MaxValue)]
    public int IdAutomovil { get; init; }

    public DateTime FechaHoraEntrada { get; init; }

    public DateTime? FechaHoraSalida { get; init; }

    public IngresoAutomovil ToEntity(int consecutivo = 0) =>
        new()
        {
            Consecutivo = consecutivo,
            IdParqueo = IdParqueo,
            IdAutomovil = IdAutomovil,
            FechaHoraEntrada = FechaHoraEntrada,
            FechaHoraSalida = FechaHoraSalida
        };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaHoraEntrada == default)
        {
            yield return new ValidationResult(
                "La fecha y hora de entrada son obligatorias.",
                [nameof(FechaHoraEntrada)]);
        }

        if (FechaHoraSalida is { } salida &&
            (salida == default || salida < FechaHoraEntrada))
        {
            yield return new ValidationResult(
                "La fecha y hora de salida deben ser válidas y no pueden preceder a la entrada.",
                [nameof(FechaHoraSalida)]);
        }
    }
}
