namespace Parqueo.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task<IReadOnlyList<TEntity>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task CrearAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> ActualizarAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
}
