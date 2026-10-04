using FamilySafety.Api.Development;
using FamilySafety.Domain.Profiles;

namespace FamilySafety.Foundation.Tests;

public sealed class MembershipInvitationLifecycleTests
{
    private static readonly DateTimeOffset IssuedAt = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InvitationExpiresAtTheExactSevenDayBoundary()
    {
        var invitation = CreateInvitation();

        Assert.True(invitation.IsAvailable(IssuedAt.AddDays(7).AddTicks(-1)));
        invitation.Expire(IssuedAt.AddDays(7));

        Assert.Equal(MembershipInvitationStatus.Expired, invitation.Status);
        Assert.False(invitation.IsAvailable(IssuedAt.AddDays(7)));
    }

    [Theory]
    [InlineData(MembershipInvitationStatus.Revoked)]
    [InlineData(MembershipInvitationStatus.Declined)]
    [InlineData(MembershipInvitationStatus.Accepted)]
    public void TerminalInvitationCannotBeUsedAgain(MembershipInvitationStatus terminalStatus)
    {
        var invitation = CreateInvitation();
        if (terminalStatus == MembershipInvitationStatus.Revoked) invitation.Revoke(IssuedAt.AddMinutes(1));
        if (terminalStatus == MembershipInvitationStatus.Declined) invitation.Decline(IssuedAt.AddMinutes(1));
        if (terminalStatus == MembershipInvitationStatus.Accepted) invitation.Accept(Guid.NewGuid(), IssuedAt.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(() => invitation.Accept(Guid.NewGuid(), IssuedAt.AddMinutes(2)));
    }

    [Fact]
    public async Task DevelopmentOutboxPlacesOpaqueTokenOnlyInFragment()
    {
        var outbox = new DevelopmentInvitationOutbox("http://localhost:4200", TimeProvider.System);

        await outbox.SendAsync("adult@example.invalid", "opaque-token", CancellationToken.None);

        var url = new Uri(outbox.Messages.Single().InvitationUrl);
        Assert.Equal(string.Empty, url.Query);
        Assert.Equal("#invite=opaque-token", url.Fragment);
    }

    private static MembershipInvitation CreateInvitation() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "adult@example.invalid",
        "ADULT@EXAMPLE.INVALID", new string('A', 64), IssuedAt, IssuedAt.AddDays(7));
}
