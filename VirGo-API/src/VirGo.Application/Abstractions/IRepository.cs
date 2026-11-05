namespace VirGo.Application.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task AddAsync(string id, T entity, CancellationToken cancellationToken = default);
}
