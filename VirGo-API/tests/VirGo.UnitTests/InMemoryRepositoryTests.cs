using VirGo.Infrastructure.Persistence.InMemory;

namespace VirGo.UnitTests;

public sealed class InMemoryRepositoryTests
{
    [Fact]
    public async Task StoresAndReadsEntitiesById()
    {
        var repository = new InMemoryRepository<TestEntity>();
        var entity = new TestEntity("one");

        await repository.AddAsync("one", entity);

        Assert.Same(entity, await repository.GetByIdAsync("one"));
        Assert.Null(await repository.GetByIdAsync("missing"));
    }

    private sealed record TestEntity(string Value);
}
