using VirGo.Application.Abstractions;

namespace VirGo.Infrastructure.Persistence.MongoDb;

public interface IMongoRepositoryFactory
{
    IRepository<T> Create<T>(string collectionName) where T : class;
}
