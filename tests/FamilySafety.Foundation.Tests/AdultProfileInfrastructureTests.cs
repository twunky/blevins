using FamilySafety.Api.Development;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FamilySafety.Foundation.Tests;

public sealed class AdultProfileInfrastructureTests
{
    [Fact]
    public void PersistenceKeepsAuthorizationConceptsSeparate()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=ModelInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var dbContext = new FamilySafetyDbContext(options);
        var entityTypes = dbContext.Model.GetEntityTypes().Select(entity => entity.ClrType).ToHashSet();

        Assert.Contains(typeof(Account), entityTypes);
        Assert.Contains(typeof(AdultProfile), entityTypes);
        Assert.Contains(typeof(FamilyGroupMembership), entityTypes);
        Assert.Contains(typeof(FamilyAdministratorAssignment), entityTypes);
    }

    [Fact]
    public void DevelopmentIdentityIsRejectedOutsideDevelopment()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DevelopmentOnlyFeatureGuard.EnsureAllowed(
                isDevelopment: false,
                enabled: true,
                "Test development identity"));

        Assert.Contains("only in Development", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RepositoryContainsTheInitialAdultProfileMigration()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=MigrationInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var dbContext = new FamilySafetyDbContext(options);

        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_InitialAdultProfile", StringComparison.Ordinal));
    }
}
