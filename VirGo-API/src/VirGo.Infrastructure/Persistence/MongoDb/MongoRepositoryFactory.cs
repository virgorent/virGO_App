using MongoDB.Driver;
using VirGo.Application.Abstractions;

namespace VirGo.Infrastructure.Persistence.MongoDb;

public sealed class MongoRepositoryFactory(IMongoDatabase database) : IMongoRepositoryFactory
{
    public IRepository<T> Create<T>(string collectionName) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        return new MongoRepository<T>(database.GetCollection<T>(collectionName));
    }

    private sealed class MongoRepository<T>(IMongoCollection<T> collection) : IRepository<T>
        where T : class
    {
        public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public Task AddAsync(string id, T entity, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            ArgumentNullException.ThrowIfNull(entity);
            return collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
        }
    }
}
