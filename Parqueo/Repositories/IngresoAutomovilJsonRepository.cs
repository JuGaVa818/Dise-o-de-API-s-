namespace Parqueo.Repositories;

public sealed class IngresoAutomovilJsonRepository(
    IRepository<Automovil> automoviles,
    IRepository<Parqueo> parqueos)
    : JsonRepositoryBase<IngresoAutomovil>(
        "ingreso_automoviles.json", item => item.Consecutivo,
        (item, id) => item.Consecutivo = id),
        IIngresoAutomovilRepository
{
    public override async Task CrearAsync(
        IngresoAutomovil entity,
        CancellationToken cancellationToken = default)
    {
        await ValidarReferenciasAsync(entity, cancellationToken);
        await base.CrearAsync(entity, cancellationToken);
    }

    public override async Task<bool> ActualizarAsync(
        IngresoAutomovil entity,
        CancellationToken cancellationToken = default)
    {
        await ValidarReferenciasAsync(entity, cancellationToken);
        return await base.ActualizarAsync(entity, cancellationToken);
    }

    public override async Task<IReadOnlyList<IngresoAutomovil>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var records = await base.ObtenerTodosAsync(cancellationToken);
        var parkById = (await parqueos.ObtenerTodosAsync(cancellationToken))
            .ToDictionary(item => item.Id);
        return records.Select(item => AsociarParqueo(item, parkById)).ToArray();
    }

    public override async Task<IngresoAutomovil?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var record = await base.ObtenerPorIdAsync(id, cancellationToken);
        if (record is null)
        {
            return null;
        }

        var park = await parqueos.ObtenerPorIdAsync(record.IdParqueo, cancellationToken);
        record.Parqueo = park ?? throw new InvalidDataException(
            $"El ingreso {record.Consecutivo} referencia un parqueo inexistente.");
        return record;
    }

    public async Task<decimal?> ObtenerPrecioPorHoraPorParqueoAsync(
        int idParqueo,
        CancellationToken cancellationToken = default) =>
        (await parqueos.ObtenerPorIdAsync(idParqueo, cancellationToken))?.PrecioPorHora;

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ListarPorTipoYFechasAsync(
        string tipo,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ValidarIntervalo(desde, hasta);
        var vehicles = await automoviles.ObtenerTodosAsync(cancellationToken);
        var vehicleById = vehicles.ToDictionary(item => item.Id);
        var records = (await ObtenerTodosAsync(cancellationToken))
            .Where(item => item.FechaHoraEntrada >= desde &&
                           item.FechaHoraEntrada <= hasta &&
                           vehicleById.TryGetValue(item.IdAutomovil, out var vehicle) &&
                           AutomovilJsonRepository.Contiene(vehicle.Tipo, tipo))
            .ToArray();
        return await MapearAsync(records, vehicleById, cancellationToken);
    }

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ListarPorProvinciaYFechasAsync(
        string provincia,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ValidarIntervalo(desde, hasta);
        var parks = await parqueos.ObtenerTodosAsync(cancellationToken);
        var parkById = parks.ToDictionary(item => item.Id);
        var vehicles = await automoviles.ObtenerTodosAsync(cancellationToken);
        var vehicleById = vehicles.ToDictionary(item => item.Id);
        var records = (await ObtenerTodosAsync(cancellationToken))
            .Where(item => item.FechaHoraEntrada >= desde &&
                           item.FechaHoraEntrada <= hasta &&
                           parkById.TryGetValue(item.IdParqueo, out var park) &&
                           AutomovilJsonRepository.Contiene(park.NombreProvincia, provincia))
            .ToArray();
        return await MapearAsync(records, vehicleById, cancellationToken);
    }

    private async Task<IReadOnlyList<IngresoAutomovilConsulta>> MapearAsync(
        IReadOnlyList<IngresoAutomovil> records,
        IReadOnlyDictionary<int, Automovil> vehicleById,
        CancellationToken cancellationToken)
    {
        var prices = new Dictionary<int, decimal?>();
        foreach (var parkId in records.Select(item => item.IdParqueo).Distinct())
        {
            prices[parkId] = await ObtenerPrecioPorHoraPorParqueoAsync(
                parkId, cancellationToken);
        }

        var result = new List<IngresoAutomovilConsulta>(records.Count);
        foreach (var item in records.OrderBy(item => item.FechaHoraEntrada))
        {
            if (!vehicleById.TryGetValue(item.IdAutomovil, out var vehicle))
            {
                throw new InvalidDataException(
                    $"El ingreso {item.Consecutivo} referencia un automóvil inexistente.");
            }

            result.Add(new IngresoAutomovilConsulta(
                item.Consecutivo,
                vehicle,
                item.FechaHoraEntrada,
                item.FechaHoraSalida,
                prices[item.IdParqueo]));
        }

        return result;
    }

    private static void ValidarIntervalo(DateTime desde, DateTime hasta)
    {
        if (desde > hasta)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la final.");
        }
    }

    private static IngresoAutomovil AsociarParqueo(
        IngresoAutomovil record,
        IReadOnlyDictionary<int, Parqueo> parkById)
    {
        if (!parkById.TryGetValue(record.IdParqueo, out var park))
        {
            throw new InvalidDataException(
                $"El ingreso {record.Consecutivo} referencia un parqueo inexistente.");
        }

        record.Parqueo = park;
        return record;
    }

    private async Task ValidarReferenciasAsync(
        IngresoAutomovil record,
        CancellationToken cancellationToken)
    {
        if (record.FechaHoraSalida is { } salida && salida < record.FechaHoraEntrada)
        {
            throw new ArgumentException("La salida no puede ser anterior a la entrada.");
        }

        if (await automoviles.ObtenerPorIdAsync(record.IdAutomovil, cancellationToken) is null)
        {
            throw new ArgumentException(
                $"No existe el automóvil {record.IdAutomovil}.");
        }

        if (await parqueos.ObtenerPorIdAsync(record.IdParqueo, cancellationToken) is null)
        {
            throw new ArgumentException(
                $"No existe el parqueo {record.IdParqueo}.");
        }
    }
}
