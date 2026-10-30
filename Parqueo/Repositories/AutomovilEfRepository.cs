using Microsoft.EntityFrameworkCore;
using Parqueo.Data;

namespace Parqueo.Repositories;

public sealed class AutomovilEfRepository(ParqueoDbContext context)
    : EfRepositoryBase<Automovil>(context), IAutomovilRepository
{
    public async Task<IReadOnlyList<Automovil>> BuscarAsync(
        string? color,
        int? anioDesde,
        int? anioHasta,
        string? fabricante,
        string? tipo,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Automoviles.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(color))
        {
            var pattern = LikePattern(color);
            query = query.Where(item => EF.Functions.ILike(item.Color, pattern, "!"));
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
            var pattern = LikePattern(fabricante);
            query = query.Where(item => EF.Functions.ILike(item.Fabricante, pattern, "!"));
        }

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var pattern = LikePattern(tipo);
            query = query.Where(item => EF.Functions.ILike(item.Tipo, pattern, "!"));
        }

        return await query.OrderBy(item => item.Id).ToListAsync(cancellationToken);
    }

    internal static string LikePattern(string value) =>
        $"%{value.Trim().Replace("!", "!!").Replace("%", "!%").Replace("_", "!_")}%";
}
