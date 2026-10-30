using Microsoft.EntityFrameworkCore;
using Parqueo.Data;

namespace Parqueo.Repositories;

public abstract class EfRepositoryBase<TEntity>(ParqueoDbContext context)
    : IRepository<TEntity> where TEntity : class
{
    protected ParqueoDbContext Context { get; } = context;
    protected DbSet<TEntity> Entities => Context.Set<TEntity>();

    public virtual async Task<IReadOnlyList<TEntity>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default) =>
        await Entities.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<TEntity?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        await Entities.FindAsync([id], cancellationToken);

    public async Task CrearAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await Entities.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ActualizarAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        var primaryKey = Context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                $"La entidad {typeof(TEntity).Name} no tiene llave primaria configurada.");
        var keyValues = primaryKey.Properties
            .Select(property => Context.Entry(entity).Property(property.Name).CurrentValue)
            .ToArray();
        var existing = await Entities.FindAsync(keyValues, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        Context.Entry(existing).CurrentValues.SetValues(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await Entities.FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return false;
        }

        Entities.Remove(entity);
        return await Context.SaveChangesAsync(cancellationToken) > 0;
    }
}
