using System.ComponentModel.DataAnnotations;

namespace Parqueo.Controllers;

public sealed class ParqueoRequest : IValidatableObject
{
    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string NombreProvincia { get; init; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Nombre { get; init; } = string.Empty;

    public decimal PrecioPorHora { get; init; }

    public Parqueo ToEntity(int id = 0) =>
        new()
        {
            Id = id,
            NombreProvincia = NombreProvincia,
            Nombre = Nombre,
            PrecioPorHora = PrecioPorHora
        };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PrecioPorHora is < 0 or > 99999999.99m)
        {
            yield return new ValidationResult(
                "El precio por hora debe estar entre 0 y 99999999.99.",
                [nameof(PrecioPorHora)]);
        }

        if (decimal.Round(PrecioPorHora, 2) != PrecioPorHora)
        {
            yield return new ValidationResult(
                "El precio por hora admite como máximo dos decimales.",
                [nameof(PrecioPorHora)]);
        }
    }
}
