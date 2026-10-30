using Microsoft.EntityFrameworkCore;
using Parqueo.Data;

namespace Parqueo.Repositories;

public sealed class ParqueoEfRepository(ParqueoDbContext context)
    : EfRepositoryBase<Parqueo>(context), IParqueoRepository
{
    public async Task<IReadOnlyList<Parqueo>> BuscarAsync(
        string? provincia,
        string? nombre,
        decimal? precioMinimo,
        decimal? precioMaximo,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Parqueos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(provincia))
        {
            var pattern = AutomovilEfRepository.LikePattern(provincia);
            query = query.Where(item =>
                EF.Functions.ILike(item.NombreProvincia, pattern, "!"));
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var pattern = AutomovilEfRepository.LikePattern(nombre);
            query = query.Where(item => EF.Functions.ILike(item.Nombre, pattern, "!"));
        }

        if (precioMinimo.HasValue)
        {
            query = query.Where(item => item.PrecioPorHora >= precioMinimo.Value);
        }

        if (precioMaximo.HasValue)
        {
            query = query.Where(item => item.PrecioPorHora <= precioMaximo.Value);
        }

        return await query.OrderBy(item => item.Id).ToListAsync(cancellationToken);
    }
}
