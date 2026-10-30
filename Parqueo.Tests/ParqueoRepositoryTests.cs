using Parqueo;
using Parqueo.Data;
using Parqueo.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Parqueo.Tests;

public sealed class ParqueoRepositoryTests
{
    [Fact]
    public void DbContextMapsOnlyTheThreeRequiredPrqTables()
    {
        var options = new DbContextOptionsBuilder<ParqueoDbContext>()
            .UseNpgsql("Host=localhost;Database=parqueo;Username=test;Password=test")
            .Options;
        using var context = new ParqueoDbContext(options);

        var tables = context.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            ["PRQ_Automoviles", "PRQ_Ingreso_Automoviles", "PRQ_Parqueo"],
            tables);
    }

    [Fact]
    public async Task JsonRepositoriesSupportFiltersCalculationsAndCrud()
    {
        var cars = new AutomovilJsonRepository();
        var parks = new ParqueoJsonRepository();
        var entries = new IngresoAutomovilJsonRepository(cars, parks);
        var start = new DateTime(2026, 10, 1);
        var end = new DateTime(2026, 10, 31, 23, 59, 59);

        Assert.Equal(5, (await cars.ObtenerTodosAsync()).Count);
        Assert.Equal(2, (await parks.ObtenerTodosAsync()).Count);
        Assert.Equal(15, (await entries.ObtenerTodosAsync()).Count);

        var filteredCars = await cars.BuscarAsync("lan", null, null, null, null);
        Assert.Single(filteredCars);
        Assert.Equal("Blanco", filteredCars[0].Color);

        var sedans = await entries.ListarPorTipoYFechasAsync("sEd", start, end);
        var completed = Assert.Single(sedans.Where(item => item.Consecutivo == 1));
        Assert.Equal(210m, completed.DuracionEstadiaMinutos);
        Assert.Equal(3.5m, completed.DuracionEstadiaHoras);
        Assert.Equal(4200m, completed.MontoTotalPagar);

        var active = Assert.Single(sedans.Where(item => item.Consecutivo == 14));
        Assert.Null(active.DuracionEstadiaMinutos);
        Assert.Null(active.DuracionEstadiaHoras);
        Assert.Null(active.MontoTotalPagar);

        var byProvince = await entries.ListarPorProvinciaYFechasAsync(
            "san jos", start, end);
        Assert.Contains(byProvince, item => item.Consecutivo == 1);
        Assert.Equal(1200m, await entries.ObtenerPrecioPorHoraPorParqueoAsync(1));

        var createdCar = new Automovil
        {
            Color = "Verde",
            Anio = 2024,
            Fabricante = "Honda",
            Tipo = "sedán"
        };
        var createdPark = new Parqueo
        {
            NombreProvincia = "Heredia",
            Nombre = "Parqueo de prueba",
            PrecioPorHora = 500m
        };
        var createdEntry = new IngresoAutomovil
        {
            IdAutomovil = 0,
            IdParqueo = 0,
            FechaHoraEntrada = new DateTime(2026, 10, 24, 10, 0, 0),
            FechaHoraSalida = new DateTime(2026, 10, 24, 11, 0, 0)
        };

        try
        {
            await cars.CrearAsync(createdCar);
            await parks.CrearAsync(createdPark);
            createdEntry.IdAutomovil = createdCar.Id;
            createdEntry.IdParqueo = createdPark.Id;
            await entries.CrearAsync(createdEntry);

            Assert.True(createdCar.Id > 0);
            Assert.True(createdPark.Id > 0);
            Assert.True(createdEntry.Consecutivo > 0);
            Assert.Equal(500m,
                (await entries.ObtenerPorIdAsync(createdEntry.Consecutivo))!.MontoTotalPagar);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => cars.EliminarAsync(createdCar.Id));
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => parks.EliminarAsync(createdPark.Id));

            createdEntry.FechaHoraSalida = null;
            Assert.True(await entries.ActualizarAsync(createdEntry));
            Assert.Null(
                (await entries.ObtenerPorIdAsync(createdEntry.Consecutivo))!.MontoTotalPagar);

            Assert.True(await entries.EliminarAsync(createdEntry.Consecutivo));
            Assert.True(await cars.EliminarAsync(createdCar.Id));
            Assert.True(await parks.EliminarAsync(createdPark.Id));
        }
        finally
        {
            if (createdEntry.Consecutivo > 0)
            {
                await entries.EliminarAsync(createdEntry.Consecutivo);
            }

            if (createdCar.Id > 0)
            {
                await cars.EliminarAsync(createdCar.Id);
            }

            if (createdPark.Id > 0)
            {
                await parks.EliminarAsync(createdPark.Id);
            }
        }
    }
}
