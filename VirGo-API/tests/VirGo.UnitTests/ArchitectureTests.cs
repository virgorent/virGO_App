using System.Reflection;

namespace VirGo.UnitTests;

public sealed class ArchitectureTests
{
    [Fact]
    public void DomainHasNoOutwardProjectDependencies()
    {
        var domainAssembly = Assembly.Load("VirGo.Domain");
        var projectReferences = domainAssembly
            .GetReferencedAssemblies()
            .Where(assembly => assembly.Name is not null && assembly.Name.StartsWith("VirGo.", StringComparison.Ordinal))
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.Empty(projectReferences);
    }
}
