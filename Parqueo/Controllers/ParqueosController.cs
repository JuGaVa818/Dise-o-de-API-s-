using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Parqueo.Repositories;

namespace Parqueo.Controllers;

[ApiController]
[Route("api/parqueos")]
public sealed class ParqueosController(IParqueoRepository parqueos) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Parqueo>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Parqueo>>> ObtenerTodos(
        [FromQuery] string? provincia,
        [FromQuery] string? nombre,
        [FromQuery] decimal? precioMinimo,
        [FromQuery] decimal? precioMaximo,
        CancellationToken cancellationToken)
    {
        if (precioMinimo > precioMaximo)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Rango de precios inválido.",
                Detail = "precioMinimo debe ser menor o igual que precioMaximo."
            });
        }

        return Ok(await parqueos.BuscarAsync(
            provincia, nombre, precioMinimo, precioMaximo, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Parqueo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Parqueo>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var parqueo = await parqueos.ObtenerPorIdAsync(id, cancellationToken);
        return parqueo is null ? NotFound() : Ok(parqueo);
    }

    [HttpPost]
    [ProducesResponseType<Parqueo>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Parqueo>> Crear(
        [FromBody] ParqueoRequest request,
        CancellationToken cancellationToken)
    {
        var parqueo = request.ToEntity();
        await parqueos.CrearAsync(parqueo, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = parqueo.Id },
            parqueo);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] ParqueoRequest request,
        CancellationToken cancellationToken)
    {
        var actualizado = await parqueos.ActualizarAsync(
            request.ToEntity(id),
            cancellationToken);
        return actualizado ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            return await parqueos.EliminarAsync(id, cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "No se puede eliminar el parqueo.",
                Detail = exception.Message
            });
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation
            })
        {
            return Conflict(new ProblemDetails
            {
                Title = "No se puede eliminar el parqueo.",
                Detail = "El parqueo tiene ingresos asociados."
            });
        }
    }
}
