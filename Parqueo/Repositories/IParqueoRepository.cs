namespace Parqueo.Repositories;

public interface IParqueoRepository : IRepository<Parqueo>
{
    Task<IReadOnlyList<Parqueo>> BuscarAsync(
        string? provincia,
        string? nombre,
        decimal? precioMinimo,
        decimal? precioMaximo,
        CancellationToken cancellationToken = default);
}
