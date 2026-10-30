namespace Parqueo.Repositories;

public interface IAutomovilRepository : IRepository<Automovil>
{
    Task<IReadOnlyList<Automovil>> BuscarAsync(
        string? color,
        int? anioDesde,
        int? anioHasta,
        string? fabricante,
        string? tipo,
        CancellationToken cancellationToken = default);
}
