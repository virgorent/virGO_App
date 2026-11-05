using System.Collections.Concurrent;
using VirGo.Application.Abstractions;

namespace VirGo.Infrastructure.Persistence.InMemory;

public sealed class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly ConcurrentDictionary<string, T> entities = new(StringComparer.Ordinal);

    public Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        entities.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public Task AddAsync(string id, T entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!entities.TryAdd(id, entity))
        {
            throw new InvalidOperationException($"An entity with id '{id}' already exists.");
        }

        return Task.CompletedTask;
    }
}
