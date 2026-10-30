using System.Text.Json.Serialization;

namespace Parqueo;

public sealed class Automovil
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("anio")]
    public int Anio { get; set; }

    [JsonPropertyName("fabricante")]
    public string Fabricante { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<IngresoAutomovil> Ingresos { get; set; } = new List<IngresoAutomovil>();
}
