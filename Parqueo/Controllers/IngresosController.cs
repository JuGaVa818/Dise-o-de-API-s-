using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Parqueo.Repositories;

namespace Parqueo.Controllers;

[ApiController]
[Route("api/ingresos")]
public sealed class IngresosController(
    IIngresoAutomovilRepository ingresos,
    IParqueoRepository parqueos,
    IAutomovilRepository automoviles) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<IngresoAutomovilResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IngresoAutomovilResponse>>> ObtenerTodos(
        CancellationToken cancellationToken)
    {
        var records = await ingresos.ObtenerTodosAsync(cancellationToken);
        return Ok(records.Select(IngresoAutomovilResponse.FromEntity).ToArray());
    }

    [HttpGet("{consecutivo:int}")]
    [ProducesResponseType<IngresoAutomovilResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngresoAutomovilResponse>> ObtenerPorConsecutivo(
        int consecutivo,
        CancellationToken cancellationToken)
    {
        var ingreso = await ingresos.ObtenerPorIdAsync(consecutivo, cancellationToken);
        return ingreso is null
            ? NotFound()
            : Ok(IngresoAutomovilResponse.FromEntity(ingreso));
    }

    [HttpPost]
    [ProducesResponseType<IngresoAutomovilResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IngresoAutomovilResponse>> Crear(
        [FromBody] IngresoAutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var errorReferencias = await ValidarReferenciasAsync(request, cancellationToken);
        if (errorReferencias is not null)
        {
            return errorReferencias;
        }

        var ingreso = request.ToEntity();
        try
        {
            await ingresos.CrearAsync(ingreso, cancellationToken);
        }
        catch (DbUpdateException exception) when (EsViolacionClaveForanea(exception))
        {
            return BadRequest(ReferenciaNoValida());
        }

        var creado = await ingresos.ObtenerPorIdAsync(
            ingreso.Consecutivo, cancellationToken)
            ?? throw new InvalidOperationException(
                $"No se encontró el ingreso {ingreso.Consecutivo} después de crearlo.");
        var response = IngresoAutomovilResponse.FromEntity(creado);
        return CreatedAtAction(
            nameof(ObtenerPorConsecutivo),
            new { consecutivo = creado.Consecutivo },
            response);
    }

    [HttpPut("{consecutivo:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        int consecutivo,
        [FromBody] IngresoAutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var errorReferencias = await ValidarReferenciasAsync(request, cancellationToken);
        if (errorReferencias is not null)
        {
            return errorReferencias;
        }

        try
        {
            var actualizado = await ingresos.ActualizarAsync(
                request.ToEntity(consecutivo),
                cancellationToken);
            return actualizado ? NoContent() : NotFound();
        }
        catch (DbUpdateException exception) when (EsViolacionClaveForanea(exception))
        {
            return BadRequest(ReferenciaNoValida());
        }
    }

    [HttpDelete("{consecutivo:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        int consecutivo,
        CancellationToken cancellationToken) =>
        await ingresos.EliminarAsync(consecutivo, cancellationToken)
            ? NoContent()
            : NotFound();

    private async Task<BadRequestObjectResult?> ValidarReferenciasAsync(
        IngresoAutomovilRequest request,
        CancellationToken cancellationToken)
    {
        if (await parqueos.ObtenerPorIdAsync(request.IdParqueo, cancellationToken) is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "El parqueo indicado no existe.",
                Detail = $"No se encontró el parqueo {request.IdParqueo}."
            });
        }

        if (await automoviles.ObtenerPorIdAsync(request.IdAutomovil, cancellationToken) is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "El automóvil indicado no existe.",
                Detail = $"No se encontró el automóvil {request.IdAutomovil}."
            });
        }

        return null;
    }

    private static bool EsViolacionClaveForanea(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation
        };

    private static ProblemDetails ReferenciaNoValida() =>
        new()
        {
            Title = "La referencia de parqueo o automóvil no existe.",
            Detail = "Verifica los IDs de parqueo y automóvil antes de registrar el ingreso."
        };
}
