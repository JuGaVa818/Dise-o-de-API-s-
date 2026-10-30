using Microsoft.EntityFrameworkCore;
using Parqueo.Data;

namespace Parqueo.Repositories;

public sealed class IngresoAutomovilEfRepository(ParqueoDbContext context)
    : EfRepositoryBase<IngresoAutomovil>(context), IIngresoAutomovilRepository
{
    public override async Task<IReadOnlyList<IngresoAutomovil>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default) =>
        await Context.IngresosAutomoviles.AsNoTracking()
            .Include(item => item.Automovil)
            .Include(item => item.Parqueo)
            .ToListAsync(cancellationToken);

    public override Task<IngresoAutomovil?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        Context.IngresosAutomoviles
            .Include(item => item.Automovil)
            .Include(item => item.Parqueo)
            .FirstOrDefaultAsync(item => item.Consecutivo == id, cancellationToken);

    public Task<decimal?> ObtenerPrecioPorHoraPorParqueoAsync(
        int idParqueo,
        CancellationToken cancellationToken = default) =>
        Context.Parqueos.AsNoTracking()
            .Where(item => item.Id == idParqueo)
            .Select(item => (decimal?)item.PrecioPorHora)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ListarPorTipoYFechasAsync(
        string tipo,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ValidarIntervalo(desde, hasta);
        var pattern = AutomovilEfRepository.LikePattern(tipo);
        var records = await BaseQuery(desde, hasta)
            .Where(item => EF.Functions.ILike(item.Automovil!.Tipo, pattern, "!"))
            .OrderBy(item => item.FechaHoraEntrada)
            .ToListAsync(cancellationToken);
        return await MapearAsync(records, cancellationToken);
    }

    public async Task<IReadOnlyList<IngresoAutomovilConsulta>> ListarPorProvinciaYFechasAsync(
        string provincia,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        ValidarIntervalo(desde, hasta);
        var pattern = AutomovilEfRepository.LikePattern(provincia);
        var records = await BaseQuery(desde, hasta)
            .Where(item => EF.Functions.ILike(item.Parqueo!.NombreProvincia, pattern, "!"))
            .OrderBy(item => item.FechaHoraEntrada)
            .ToListAsync(cancellationToken);
        return await MapearAsync(records, cancellationToken);
    }

    private IQueryable<IngresoAutomovil> BaseQuery(DateTime desde, DateTime hasta) =>
        Context.IngresosAutomoviles.AsNoTracking()
            .Include(item => item.Automovil)
            .Include(item => item.Parqueo)
            .Where(item => item.FechaHoraEntrada >= desde && item.FechaHoraEntrada <= hasta);

    private async Task<IReadOnlyList<IngresoAutomovilConsulta>> MapearAsync(
        IReadOnlyList<IngresoAutomovil> records,
        CancellationToken cancellationToken)
    {
        var precios = new Dictionary<int, decimal?>();
        foreach (var idParqueo in records.Select(item => item.IdParqueo).Distinct())
        {
            precios[idParqueo] = await ObtenerPrecioPorHoraPorParqueoAsync(
                idParqueo, cancellationToken);
        }

        return records.Select(item => new IngresoAutomovilConsulta(
            item.Consecutivo,
            item.Automovil ?? throw new InvalidOperationException(
                $"El ingreso {item.Consecutivo} no tiene automóvil relacionado."),
            item.FechaHoraEntrada,
            item.FechaHoraSalida,
            precios[item.IdParqueo])).ToArray();
    }

    private static void ValidarIntervalo(DateTime desde, DateTime hasta)
    {
        if (desde > hasta)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la final.");
        }
    }
}
