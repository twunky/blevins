using FamilySafety.Application.Invitations;
using FamilySafety.Application.Profiles;

namespace FamilySafety.Foundation.Tests;

public sealed class MembershipInvitationServiceTests
{
    private static readonly Guid AccountId = Guid.NewGuid();
    private static readonly Guid GroupId = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AcceptanceReturnsOnlyCommittedFamilyIdentity()
    {
        var accepted = await BuildService(new FakeRepository(), new FakeDelivery())
            .AcceptAsync(new InvitationTokenRequest("opaque"), CancellationToken.None);
        Assert.Equal(GroupId, accepted.FamilyGroupId);
    }

    [Theory]
    [InlineData(InvitationRepositoryOutcome.Unavailable, typeof(MembershipInvitationUnavailableException))]
    [InlineData(InvitationRepositoryOutcome.CapacityReached, typeof(MembershipInvitationCapacityException))]
    public async Task FailedAcceptanceDoesNotReturnFamilyIdentity(InvitationRepositoryOutcome outcome, Type exceptionType)
    {
        var service = BuildService(new FakeRepository { Outcome = outcome }, new FakeDelivery());
        var exception = await Record.ExceptionAsync(() => service.AcceptAsync(new InvitationTokenRequest("opaque"), CancellationToken.None));
        Assert.IsType(exceptionType, exception);
    }

    [Fact]
    public async Task IssueNormalizesEmailStoresOnlyHashAndDeliversRawToken()
    {
        var repository = new FakeRepository();
        var delivery = new FakeDelivery();
        var service = BuildService(repository, delivery);

        await service.IssueAsync(
            new IssueMembershipInvitationRequest(GroupId, " Adult@Example.com "), CancellationToken.None);

        Assert.Equal("ADULT@EXAMPLE.COM", repository.NormalizedEmail);
        Assert.Equal(64, repository.TokenHash?.Length);
        Assert.NotNull(delivery.Token);
        Assert.NotEqual(delivery.Token, repository.TokenHash);
        Assert.Equal(Now.AddDays(7), repository.ExpiresAt);
    }

    [Fact]
    public async Task InvalidEmailReturnsFieldSpecificValidation()
    {
        var service = BuildService(new FakeRepository(), new FakeDelivery());

        var exception = await Assert.ThrowsAsync<MembershipInvitationValidationException>(() =>
            service.IssueAsync(new IssueMembershipInvitationRequest(GroupId, "not-email"), CancellationToken.None));

        Assert.Contains("recipientEmail", exception.Errors.Keys);
    }

    [Theory]
    [InlineData(InvitationRepositoryOutcome.Unavailable, typeof(MembershipInvitationUnavailableException))]
    [InlineData(InvitationRepositoryOutcome.CapacityReached, typeof(MembershipInvitationCapacityException))]
    public async Task RepositoryFailuresMapToStableApplicationErrors(
        InvitationRepositoryOutcome outcome,
        Type exceptionType)
    {
        var repository = new FakeRepository { Outcome = outcome };
        var service = BuildService(repository, new FakeDelivery());

        var exception = await Record.ExceptionAsync(() => service.IssueAsync(
            new IssueMembershipInvitationRequest(GroupId, "adult@example.com"), CancellationToken.None));

        Assert.IsType(exceptionType, exception);
    }

    private static MembershipInvitationService BuildService(FakeRepository repository, FakeDelivery delivery) =>
        new(new CurrentAccessor(), repository, delivery, new FixedTimeProvider(Now));

    private sealed class CurrentAccessor : ICurrentAccountAccessor
    {
        public CurrentAccount Current { get; } = new(AccountId);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeDelivery : IMembershipInvitationDelivery
    {
        public string? Token { get; private set; }
        public Task SendAsync(string recipientEmail, string token, CancellationToken cancellationToken)
        {
            Token = token;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRepository : IMembershipInvitationRepository
    {
        public InvitationRepositoryOutcome Outcome { get; init; } = InvitationRepositoryOutcome.Success;
        public string? NormalizedEmail { get; private set; }
        public string? TokenHash { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }

        public Task<IReadOnlyList<AdministeredFamilyGroupView>> GetAdministeredGroupsAsync(Guid accountId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AdministeredFamilyGroupView>>([]);
        public Task<InvitationRepositoryOutcome> IssueAsync(Guid accountId, Guid familyGroupId, string recipientEmail, string normalizedRecipientEmail, string tokenHash, DateTimeOffset issuedAt, DateTimeOffset expiresAt, CancellationToken cancellationToken)
        {
            NormalizedEmail = normalizedRecipientEmail; TokenHash = tokenHash; ExpiresAt = expiresAt;
            return Task.FromResult(Outcome);
        }
        public Task<InvitationRepositoryOutcome> RevokeAsync(Guid accountId, Guid familyGroupId, string normalizedRecipientEmail, DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(Outcome);
        public Task<InvitationPreviewData> PreviewAsync(Guid accountId, string tokenHash, DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(new InvitationPreviewData(Outcome, "Family", "Inviter"));
        public Task<InvitationAcceptanceData> AcceptAsync(Guid accountId, string tokenHash, DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(new InvitationAcceptanceData(Outcome, Outcome == InvitationRepositoryOutcome.Success ? GroupId : null));
        public Task<InvitationRepositoryOutcome> DeclineAsync(Guid accountId, string tokenHash, DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(Outcome);
    }
}
