namespace Parqueo.Repositories;

public sealed class AutomovilJsonRepository
    : JsonRepositoryBase<Automovil>,
        IAutomovilRepository
{
    private readonly JsonRepositoryBase<IngresoAutomovil> _ingresos =
        new JsonRepositoryBase<IngresoAutomovil>(
            "ingreso_automoviles.json", item => item.Consecutivo);

    public AutomovilJsonRepository()
        : base("automoviles.json", item => item.Id, (item, id) => item.Id = id)
    {
    }

    public override async Task CrearAsync(
        Automovil entity,
        CancellationToken cancellationToken = default)
    {
        Validar(entity);
        await base.CrearAsync(entity, cancellationToken);
    }

    public override async Task<bool> ActualizarAsync(
        Automovil entity,
        CancellationToken cancellationToken = default)
    {
        Validar(entity);
        return await base.ActualizarAsync(entity, cancellationToken);
    }

    public override async Task<bool> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if ((await _ingresos.ObtenerTodosAsync(cancellationToken))
            .Any(item => item.IdAutomovil == id))
        {
            throw new InvalidOperationException(
                $"No se puede eliminar el automóvil {id}: tiene ingresos asociados.");
        }

        return await base.EliminarAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<Automovil>> BuscarAsync(
        string? color,
        int? anioDesde,
        int? anioHasta,
        string? fabricante,
        string? tipo,
        CancellationToken cancellationToken = default)
    {
        var query = (await ObtenerTodosAsync(cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(color))
        {
            query = query.Where(item => Contiene(item.Color, color));
        }

        if (anioDesde.HasValue)
        {
            query = query.Where(item => item.Anio >= anioDesde.Value);
        }

        if (anioHasta.HasValue)
        {
            query = query.Where(item => item.Anio <= anioHasta.Value);
        }

        if (!string.IsNullOrWhiteSpace(fabricante))
        {
            query = query.Where(item => Contiene(item.Fabricante, fabricante));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            query = query.Where(item => Contiene(item.Tipo, tipo));
        }

        return query.OrderBy(item => item.Id).ToArray();
    }

    internal static bool Contiene(string value, string filter) =>
        value.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    private static void Validar(Automovil entity)
    {
        if (entity.Anio is < 1886 or > 2100 ||
            entity.Tipo is not ("sedán" or "4x4" or "moto") ||
            string.IsNullOrWhiteSpace(entity.Color) || entity.Color.Length > 40 ||
            string.IsNullOrWhiteSpace(entity.Fabricante) || entity.Fabricante.Length > 80)
        {
            throw new ArgumentException("Los datos del automóvil no cumplen el esquema PRQ.");
        }
    }
}
