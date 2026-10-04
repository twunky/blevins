using System.Reflection;

namespace FamilySafety.Foundation.Tests;

public sealed class DependencyDirectionTests
{
    [Fact]
    public void DomainDoesNotReferenceOtherApplicationLayers()
    {
        var references = ReferencedProjectNames(FamilySafety.Domain.AssemblyReference.Assembly);

        Assert.DoesNotContain("FamilySafety.Application", references);
        Assert.DoesNotContain("FamilySafety.Infrastructure", references);
        Assert.DoesNotContain("FamilySafety.Api", references);
    }

    [Fact]
    public void ApplicationDoesNotReferenceInfrastructureOrApi()
    {
        var references = ReferencedProjectNames(FamilySafety.Application.AssemblyReference.Assembly);

        Assert.DoesNotContain("FamilySafety.Infrastructure", references);
        Assert.DoesNotContain("FamilySafety.Api", references);
    }

    [Fact]
    public void InfrastructureDoesNotReferenceApi()
    {
        var references = ReferencedProjectNames(FamilySafety.Infrastructure.AssemblyReference.Assembly);

        Assert.DoesNotContain("FamilySafety.Api", references);
    }

    private static HashSet<string> ReferencedProjectNames(Assembly assembly) =>
        assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && name.StartsWith("FamilySafety.", StringComparison.Ordinal))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);
}
