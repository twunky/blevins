using FamilySafety.Application.Events;
using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Events;

namespace FamilySafety.Foundation.Tests;

public sealed class EventInvitationServiceTests
{
    [Theory]
    [InlineData("organizer")]
    [InlineData("own")]
    [InlineData("pending")]
    [InlineData("issue")]
    [InlineData("revoke")]
    [InlineData("respond")]
    public async Task EveryOperationRequiresTheCurrentAuthenticatedActor(string operation)
    {
        var repository = new EventInvitationApiTests.Repository();
        var service = new EventInvitationService(repository, new EventInvitationApiTests.Accessor());
        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() => Invoke(service, operation));
        Assert.Equal(Guid.Empty, repository.LastAccount);
    }

    [Theory]
    [InlineData("organizer")]
    [InlineData("own")]
    [InlineData("pending")]
    [InlineData("issue")]
    [InlineData("revoke")]
    [InlineData("respond")]
    public async Task UnavailableAndMissingShareOneResultAndActorComesOnlyFromSession(string operation)
    {
        var actor = Guid.NewGuid();
        var repository = new EventInvitationApiTests.Repository { Unavailable = true };
        var service = new EventInvitationService(repository, new EventInvitationApiTests.Accessor { Current = new(actor) });
        await Assert.ThrowsAsync<FamilyEventUnavailableException>(() => Invoke(service, operation));
        Assert.Equal(actor, repository.LastAccount);
    }

    private static async Task Invoke(EventInvitationService service, string operation)
    {
        var id = Guid.NewGuid();
        switch (operation)
        {
            case "organizer": await service.GetOrganizerAsync(id, CancellationToken.None); break;
            case "own": await service.GetOwnAsync(id, CancellationToken.None); break;
            case "pending": await service.GetPendingAsync(id, CancellationToken.None); break;
            case "issue": await service.IssueAsync(id, new(Guid.NewGuid()), CancellationToken.None); break;
            case "revoke": await service.RevokeAsync(id, Guid.NewGuid(), new(Guid.NewGuid()), CancellationToken.None); break;
            default: await service.RespondAsync(id, new(Guid.NewGuid(), Guid.NewGuid(), EventRsvpStatus.Going), CancellationToken.None); break;
        }
    }
}
