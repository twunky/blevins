using FamilySafety.Domain.Profiles;
using FamilySafety.Domain.Trips;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyTripInfrastructureTests
{
    [Fact]
    public void TripTablesAndMembershipVersionAreInModelAndMigration()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var db = new FamilySafetyDbContext(options);
        Assert.NotNull(db.Model.FindEntityType(typeof(FamilyTrip)));
        Assert.NotNull(db.Model.FindEntityType(typeof(FamilyTripTraveler)));
        Assert.NotNull(db.Model.FindEntityType(typeof(FamilyTripAudienceMember)));
        Assert.NotNull(db.Model.FindEntityType(typeof(FamilyTripContact)));
        Assert.NotNull(db.Model.FindEntityType(typeof(FamilyGroupMembership))!
            .FindProperty(nameof(FamilyGroupMembership.AccessVersion)));
        Assert.Contains(db.Database.GetMigrations(), migration =>
            migration.EndsWith("_TripCreationAudienceDetail", StringComparison.Ordinal));
        var script = db.GetService<IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE [FamilyTrips]", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE [FamilyTripAudienceMembers]", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[AccessVersion]", script, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EndingMembershipAdvancesVersionOnlyOnce()
    {
        var member = new FamilyGroupMembership(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true);
        Assert.Equal(0, member.AccessVersion);
        member.End();
        member.End();
        Assert.False(member.IsActive);
        Assert.Equal(1, member.AccessVersion);
    }
}
