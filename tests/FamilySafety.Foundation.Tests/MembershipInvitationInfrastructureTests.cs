using FamilySafety.Api.Development;
using FamilySafety.Domain.Profiles;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FamilySafety.Foundation.Tests;

public sealed class MembershipInvitationInfrastructureTests
{
    [Fact]
    public void ModelEnforcesInvitationAndMembershipConcurrencyBoundaries()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var dbContext = new FamilySafetyDbContext(options);
        var invitation = dbContext.Model.FindEntityType(typeof(MembershipInvitation))!;

        Assert.True(invitation.FindProperty(nameof(MembershipInvitation.RowVersion))!.IsConcurrencyToken);
        Assert.Contains(invitation.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(MembershipInvitation.TokenHash)]));
        Assert.Contains(invitation.GetIndexes(), index =>
            index.IsUnique && index.GetFilter() == "[Status] = N'Pending'");

        var membership = dbContext.Model.FindEntityType(typeof(FamilyGroupMembership))!;
        Assert.Contains(membership.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(FamilyGroupMembership.AccountId), nameof(FamilyGroupMembership.FamilyGroupId)]));
    }

    [Fact]
    public void DevelopmentOutboxIsRejectedOutsideDevelopment()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DevelopmentOnlyFeatureGuard.EnsureAllowed(false, true, "Development invitation outbox"));
        Assert.Contains("only in Development", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RepositoryContainsInvitationMigration()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer("Server=localhost;Database=MigrationInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var dbContext = new FamilySafetyDbContext(options);
        Assert.Contains(dbContext.Database.GetMigrations(), migration =>
            migration.EndsWith("_AdultMembershipInvitations", StringComparison.Ordinal));
    }
}
