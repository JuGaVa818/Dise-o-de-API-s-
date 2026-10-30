namespace Parqueo.Repositories;

public interface IIngresoAutomovilRepository : IRepository<IngresoAutomovil>
{
    Task<decimal?> ObtenerPrecioPorHoraPorParqueoAsync(
        int idParqueo,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngresoAutomovilConsulta>> ListarPorTipoYFechasAsync(
        string tipo,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngresoAutomovilConsulta>> ListarPorProvinciaYFechasAsync(
        string provincia,
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default);
}
