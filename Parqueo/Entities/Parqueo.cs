using System.Text.Json.Serialization;

namespace Parqueo;

public sealed class Parqueo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nombre_provincia")]
    public string NombreProvincia { get; set; } = string.Empty;

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonPropertyName("precio_por_hora")]
    public decimal PrecioPorHora { get; set; }

    [JsonIgnore]
    public ICollection<IngresoAutomovil> Ingresos { get; set; } = new List<IngresoAutomovil>();
}
