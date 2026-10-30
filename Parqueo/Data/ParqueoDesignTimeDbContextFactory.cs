using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Parqueo.Data;

public sealed class ParqueoDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ParqueoDbContext>
{
    public ParqueoDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(ParqueoDbContext).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("Parqueo");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Falta ConnectionStrings:Parqueo en User Secrets o en variables de entorno.");
        }

        var options = new DbContextOptionsBuilder<ParqueoDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new ParqueoDbContext(options);
    }
}
