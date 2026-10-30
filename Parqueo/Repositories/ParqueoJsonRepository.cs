namespace Parqueo.Repositories;

public sealed class ParqueoJsonRepository
    : JsonRepositoryBase<Parqueo>,
        IParqueoRepository
{
    private readonly JsonRepositoryBase<IngresoAutomovil> _ingresos =
        new JsonRepositoryBase<IngresoAutomovil>(
            "ingreso_automoviles.json", item => item.Consecutivo);

    public ParqueoJsonRepository()
        : base("parqueo.json", item => item.Id, (item, id) => item.Id = id)
    {
    }

    public override async Task CrearAsync(
        Parqueo entity,
        CancellationToken cancellationToken = default)
    {
        Validar(entity);
        await base.CrearAsync(entity, cancellationToken);
    }

    public override async Task<bool> ActualizarAsync(
        Parqueo entity,
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
            .Any(item => item.IdParqueo == id))
        {
            throw new InvalidOperationException(
                $"No se puede eliminar el parqueo {id}: tiene ingresos asociados.");
        }

        return await base.EliminarAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<Parqueo>> BuscarAsync(
        string? provincia,
        string? nombre,
        decimal? precioMinimo,
        decimal? precioMaximo,
        CancellationToken cancellationToken = default)
    {
        var query = (await ObtenerTodosAsync(cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(provincia))
        {
            query = query.Where(item =>
                AutomovilJsonRepository.Contiene(item.NombreProvincia, provincia));
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            query = query.Where(item =>
                AutomovilJsonRepository.Contiene(item.Nombre, nombre));
        }

        if (precioMinimo.HasValue)
        {
            query = query.Where(item => item.PrecioPorHora >= precioMinimo.Value);
        }

        if (precioMaximo.HasValue)
        {
            query = query.Where(item => item.PrecioPorHora <= precioMaximo.Value);
        }

        return query.OrderBy(item => item.Id).ToArray();
    }

    private static void Validar(Parqueo entity)
    {
        if (string.IsNullOrWhiteSpace(entity.NombreProvincia) ||
            entity.NombreProvincia.Length > 80 ||
            string.IsNullOrWhiteSpace(entity.Nombre) || entity.Nombre.Length > 120 ||
            entity.PrecioPorHora < 0)
        {
            throw new ArgumentException("Los datos del parqueo no cumplen el esquema PRQ.");
        }
    }
}
