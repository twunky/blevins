using FamilySafety.Domain.Events;
using FamilySafety.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyEventInfrastructureTests
{
    [Fact]
    public void EventTablesAndAudienceKeyAreInTheModel()
    {
        var options = new DbContextOptionsBuilder<FamilySafetyDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=ModelInspection;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var dbContext = new FamilySafetyDbContext(options);

        Assert.Contains(typeof(FamilyEvent), dbContext.Model.GetEntityTypes().Select(entity => entity.ClrType));
        Assert.Contains(
            typeof(FamilyEventAudienceMember),
            dbContext.Model.GetEntityTypes().Select(entity => entity.ClrType));
        Assert.Equal(
            ["EventId", "ProfileId"],
            dbContext.Model.FindEntityType(typeof(FamilyEventAudienceMember))!
                .FindPrimaryKey()!.Properties.Select(property => property.Name));
        Assert.Contains(
            "CanEdit",
            dbContext.Model.FindEntityType(typeof(FamilyEventAudienceMember))!
                .GetProperties().Select(property => property.Name));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_EventAudienceSelection", StringComparison.Ordinal));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_EventEditAccess", StringComparison.Ordinal));
        var script = dbContext.GetService<IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE [FamilyEvents]", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE [FamilyEventAudienceMembers]", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ADD [CanEdit] bit", script, StringComparison.OrdinalIgnoreCase);
    }
}
