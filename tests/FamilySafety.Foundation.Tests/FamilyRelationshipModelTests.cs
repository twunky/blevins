using FamilySafety.Domain.Relationships;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyRelationshipModelTests
{
    [Fact]
    public void ModelEnforcesUniqueFamilyGroupAndOrderIndependentMemberPair()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(FamilyRelationship));
        Assert.NotNull(entity);

        var pairIndex = Assert.Single(entity.GetIndexes(), index => index.IsUnique);
        Assert.Equal(
            [nameof(FamilyRelationship.FamilyGroupId), nameof(FamilyRelationship.FirstProfileId), nameof(FamilyRelationship.SecondProfileId)],
            pairIndex.Properties.Select(property => property.Name));
        Assert.Contains(entity.GetForeignKeys(), key =>
            key.Properties.Single().Name == nameof(FamilyRelationship.ProposerProfileId)
            && key.DeleteBehavior == DeleteBehavior.Restrict);
        Assert.Contains(entity.GetForeignKeys(), key =>
            key.Properties.Single().Name == nameof(FamilyRelationship.RecipientProfileId)
            && key.DeleteBehavior == DeleteBehavior.Restrict);
    }

    [Fact]
    public void MigrationAssemblyContainsFamilyRelationshipMigration()
    {
        using var context = CreateContext();
        var migrationsAssembly = context.GetService<IMigrationsAssembly>();
        const string migrationId = "20260926120000_AdultFamilyRelationships";
        Assert.Contains(migrationId, migrationsAssembly.Migrations.Keys);
        var migrationType = migrationsAssembly.Migrations[migrationId];
        var migration = migrationsAssembly.CreateMigration(migrationType, context.Database.ProviderName!);
        Assert.NotNull(migration.TargetModel.FindEntityType(typeof(FamilyRelationship)));

        var script = context.GetService<IMigrator>().GenerateScript(
            "20260926012828_AdultMembershipInvitations", migrationId);
        Assert.Contains("CREATE TABLE [FamilyRelationships]", script, StringComparison.Ordinal);
        Assert.Contains("CREATE UNIQUE INDEX [IX_FamilyRelationships_FamilyGroupId_FirstProfileId_SecondProfileId]", script, StringComparison.Ordinal);
    }

    private static FamilySafetyDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new FamilySafetyDbContext(options);
    }
}
