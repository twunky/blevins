using FamilySafety.Application.Profiles;
using FamilySafety.Application.Relationships;
using FamilySafety.Domain.Relationships;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyRelationshipServiceTests
{
    [Fact]
    public async Task ProposalUsesCurrentAccountAndApprovedLabel()
    {
        var accountId = Guid.NewGuid();
        var repository = new FakeRelationshipRepository();
        var service = new FamilyRelationshipService(new CurrentAccountAccessor(accountId), repository);
        var groupId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        await service.ProposeAsync(
            groupId,
            new ProposeFamilyRelationshipRequest(recipientId, FamilyRelationshipLabel.Parent),
            CancellationToken.None);

        Assert.Equal(accountId, repository.AccountId);
        Assert.Equal(groupId, repository.GroupId);
        Assert.Equal(recipientId, repository.RecipientProfileId);
        Assert.Equal(FamilyRelationshipLabel.Parent, repository.ProposerLabel);
    }

    [Fact]
    public async Task UnsupportedLabelIsRejectedBeforeRepositoryCall()
    {
        var repository = new FakeRelationshipRepository();
        var service = new FamilyRelationshipService(new CurrentAccountAccessor(Guid.NewGuid()), repository);

        await Assert.ThrowsAsync<FamilyRelationshipValidationException>(() => service.ProposeAsync(
            Guid.NewGuid(),
            new ProposeFamilyRelationshipRequest(Guid.NewGuid(), (FamilyRelationshipLabel)999),
            CancellationToken.None));

        Assert.Null(repository.AccountId);
    }

    [Fact]
    public async Task MissingCurrentAccountIsUnavailable()
    {
        var service = new FamilyRelationshipService(new CurrentAccountAccessor(null), new FakeRelationshipRepository());

        await Assert.ThrowsAsync<CurrentAccountUnavailableException>(() =>
            service.GetForGroupAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private sealed class CurrentAccountAccessor(Guid? accountId) : ICurrentAccountAccessor
    {
        public CurrentAccount? Current => accountId is null ? null : new CurrentAccount(accountId.Value);
    }

    private sealed class FakeRelationshipRepository : IFamilyRelationshipRepository
    {
        public Guid? AccountId { get; private set; }
        public Guid GroupId { get; private set; }
        public Guid RecipientProfileId { get; private set; }
        public FamilyRelationshipLabel ProposerLabel { get; private set; }

        public Task<IReadOnlyList<FamilyRelationshipView>?> GetForGroupAsync(
            Guid accountId,
            Guid familyGroupId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FamilyRelationshipView>?>([]);

        public Task<FamilyRelationshipOperationResult> ProposeAsync(
            Guid accountId,
            Guid familyGroupId,
            Guid recipientProfileId,
            FamilyRelationshipLabel proposerLabel,
            CancellationToken cancellationToken)
        {
            AccountId = accountId;
            GroupId = familyGroupId;
            RecipientProfileId = recipientProfileId;
            ProposerLabel = proposerLabel;
            return Task.FromResult(FamilyRelationshipOperationResult.Success);
        }

        public Task<FamilyRelationshipOperationResult> RespondAsync(
            Guid accountId,
            Guid relationshipId,
            FamilyRelationshipAction action,
            CancellationToken cancellationToken) =>
            Task.FromResult(FamilyRelationshipOperationResult.Success);
    }
}
