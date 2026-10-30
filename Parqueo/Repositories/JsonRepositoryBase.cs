using System.Collections.Concurrent;
using System.Text.Json;

namespace Parqueo.Repositories;

public class JsonRepositoryBase<TEntity>(
    string fileName,
    Func<TEntity, int> key,
    Action<TEntity, int>? assignKey = null)
    : IRepository<TEntity> where TEntity : class
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    private readonly string _path = Path.Combine(AppContext.BaseDirectory, fileName);
    private readonly SemaphoreSlim _fileLock = FileLocks.GetOrAdd(
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, fileName)),
        _ => new SemaphoreSlim(1, 1));

    public virtual async Task<IReadOnlyList<TEntity>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return (await LeerAsync(cancellationToken)).ToArray();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public virtual async Task<TEntity?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return (await LeerAsync(cancellationToken)).FirstOrDefault(item => key(item) == id);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public virtual async Task CrearAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var records = await LeerAsync(cancellationToken);
            if (key(entity) <= 0 && assignKey is not null)
            {
                assignKey(entity, records.Count == 0 ? 1 : records.Max(key) + 1);
            }

            if (records.Any(item => key(item) == key(entity)))
            {
                throw new InvalidOperationException($"Ya existe el registro con llave {key(entity)}.");
            }

            records.Add(entity);
            await GuardarAsync(records, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public virtual async Task<bool> ActualizarAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var records = await LeerAsync(cancellationToken);
            var index = records.FindIndex(item => key(item) == key(entity));
            if (index < 0)
            {
                return false;
            }

            records[index] = entity;
            await GuardarAsync(records, cancellationToken);
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public virtual async Task<bool> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var records = await LeerAsync(cancellationToken);
            var removed = records.RemoveAll(item => key(item) == id);
            if (removed == 0)
            {
                return false;
            }

            await GuardarAsync(records, cancellationToken);
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    protected async Task<List<TEntity>> LeerAsync(CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            _path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);
        return await JsonSerializer.DeserializeAsync<List<TEntity>>(
                   stream, JsonOptions, cancellationToken)
               ?? throw new JsonException($"El archivo {_path} no contiene una lista JSON válida.");
    }

    private async Task GuardarAsync(
        List<TEntity> records,
        CancellationToken cancellationToken)
    {
        var temporaryPath = $"{_path}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                             bufferSize: 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, records, JsonOptions, cancellationToken);
            }

            File.Move(temporaryPath, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
