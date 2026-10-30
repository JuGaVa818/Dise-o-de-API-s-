using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Parqueo.Data;
using Parqueo.Repositories;
using AutomovilModel = global::Parqueo.Automovil;
using ParqueoModel = global::Parqueo.Parqueo;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

var storageMode = builder.Configuration["Storage:Mode"] ?? "json";
var connectionString = builder.Configuration.GetConnectionString("Parqueo");

if (storageMode.Equals("postgres", StringComparison.OrdinalIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Falta ConnectionStrings:Parqueo en User Secrets. Consulta README.md.");
    }

    builder.Services.AddDbContext<ParqueoDbContext>(
        options => options.UseNpgsql(connectionString));
}
else if (!storageMode.Equals("json", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        $"Modo de almacenamiento no reconocido: {storageMode}. Usa json o postgres.");
}

builder.Services.AddScoped<AutomovilEfRepository>();
builder.Services.AddScoped<ParqueoEfRepository>();
builder.Services.AddScoped<IngresoAutomovilEfRepository>();
builder.Services.AddScoped<AutomovilJsonRepository>();
builder.Services.AddScoped<ParqueoJsonRepository>();
builder.Services.AddScoped<IngresoAutomovilJsonRepository>();
builder.Services.AddScoped<IAutomovilRepository>(services =>
    storageMode.Equals("json", StringComparison.OrdinalIgnoreCase)
        ? services.GetRequiredService<AutomovilJsonRepository>()
        : services.GetRequiredService<AutomovilEfRepository>());
builder.Services.AddScoped<IParqueoRepository>(services =>
    storageMode.Equals("json", StringComparison.OrdinalIgnoreCase)
        ? services.GetRequiredService<ParqueoJsonRepository>()
        : services.GetRequiredService<ParqueoEfRepository>());
builder.Services.AddScoped<IIngresoAutomovilRepository>(services =>
    storageMode.Equals("json", StringComparison.OrdinalIgnoreCase)
        ? services.GetRequiredService<IngresoAutomovilJsonRepository>()
        : services.GetRequiredService<IngresoAutomovilEfRepository>());
builder.Services.AddScoped<IRepository<AutomovilModel>>(services =>
    storageMode.Equals("json", StringComparison.OrdinalIgnoreCase)
        ? services.GetRequiredService<AutomovilJsonRepository>()
        : services.GetRequiredService<AutomovilEfRepository>());
builder.Services.AddScoped<IRepository<ParqueoModel>>(services =>
    storageMode.Equals("json", StringComparison.OrdinalIgnoreCase)
        ? services.GetRequiredService<ParqueoJsonRepository>()
        : services.GetRequiredService<ParqueoEfRepository>());

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var automoviles = scope.ServiceProvider.GetRequiredService<IAutomovilRepository>();
var parqueos = scope.ServiceProvider.GetRequiredService<IParqueoRepository>();
var ingresos = scope.ServiceProvider.GetRequiredService<IIngresoAutomovilRepository>();

var automovilCount = (await automoviles.ObtenerTodosAsync()).Count;
var parqueoCount = (await parqueos.ObtenerTodosAsync()).Count;
var ingresoCount = (await ingresos.ObtenerTodosAsync()).Count;
Console.WriteLine($"Parqueo ({storageMode}): {automovilCount} automóviles, " +
                  $"{parqueoCount} parqueos, {ingresoCount} ingresos.");
