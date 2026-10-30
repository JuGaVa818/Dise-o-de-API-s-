using System.ComponentModel.DataAnnotations;

namespace Parqueo.Controllers;

public sealed class AutomovilRequest
{
    [Required]
    [StringLength(40, MinimumLength = 1)]
    public string Color { get; init; } = string.Empty;

    [Range(1886, 2100)]
    public int Anio { get; init; }

    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Fabricante { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^(sedán|4x4|moto)$")]
    public string Tipo { get; init; } = string.Empty;

    public Automovil ToEntity(int id = 0) =>
        new()
        {
            Id = id,
            Color = Color,
            Anio = Anio,
            Fabricante = Fabricante,
            Tipo = Tipo
        };
}
