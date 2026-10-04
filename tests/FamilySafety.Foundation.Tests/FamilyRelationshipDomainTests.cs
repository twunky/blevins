using FamilySafety.Domain.Relationships;

namespace FamilySafety.Foundation.Tests;

public sealed class FamilyRelationshipDomainTests
{
    [Theory]
    [InlineData(FamilyRelationshipLabel.Parent, FamilyRelationshipLabel.AdultChild)]
    [InlineData(FamilyRelationshipLabel.AdultChild, FamilyRelationshipLabel.Parent)]
    [InlineData(FamilyRelationshipLabel.SpousePartner, FamilyRelationshipLabel.SpousePartner)]
    [InlineData(FamilyRelationshipLabel.Sibling, FamilyRelationshipLabel.Sibling)]
    [InlineData(FamilyRelationshipLabel.OtherFamily, FamilyRelationshipLabel.OtherFamily)]
    public void ReciprocalLabelsAreDefined(FamilyRelationshipLabel label, FamilyRelationshipLabel reciprocal)
    {
        Assert.Equal(reciprocal, FamilyRelationship.ReciprocalOf(label));
    }

    [Fact]
    public void ProposalStoresOrderIndependentPairAndBeginsPending()
    {
        var proposer = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var recipient = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var relationship = new FamilyRelationship(
            Guid.NewGuid(), Guid.NewGuid(), proposer, recipient, FamilyRelationshipLabel.Parent);

        Assert.Equal(recipient, relationship.FirstProfileId);
        Assert.Equal(proposer, relationship.SecondProfileId);
        Assert.Equal(FamilyRelationshipStatus.Pending, relationship.Status);
        relationship.Accept(recipient);
        Assert.Equal(FamilyRelationshipStatus.Accepted, relationship.Status);
    }

    [Fact]
    public void SelfRelationshipIsRejected()
    {
        var profileId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new FamilyRelationship(
            Guid.NewGuid(), Guid.NewGuid(), profileId, profileId, FamilyRelationshipLabel.Sibling));
    }

    [Fact]
    public void UnsupportedLabelIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FamilyRelationship(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), (FamilyRelationshipLabel)999));
    }

    [Fact]
    public void OnlyRecipientCanAcceptPendingProposal()
    {
        var proposer = Guid.NewGuid();
        var recipient = Guid.NewGuid();
        var relationship = new FamilyRelationship(
            Guid.NewGuid(), Guid.NewGuid(), proposer, recipient, FamilyRelationshipLabel.Sibling);

        Assert.Throws<InvalidOperationException>(() => relationship.Accept(proposer));
        relationship.Accept(recipient);
        Assert.Throws<InvalidOperationException>(() => relationship.Accept(recipient));
    }

    [Fact]
    public void PendingAndAcceptedActionsAreLimitedToTheirAuthorizedParticipants()
    {
        var proposer = Guid.NewGuid();
        var recipient = Guid.NewGuid();
        var otherProfile = Guid.NewGuid();
        var pending = new FamilyRelationship(
            Guid.NewGuid(), Guid.NewGuid(), proposer, recipient, FamilyRelationshipLabel.Parent);

        Assert.Throws<InvalidOperationException>(() => pending.EnsureCanDecline(proposer));
        Assert.Throws<InvalidOperationException>(() => pending.EnsureCanWithdraw(recipient));
        pending.EnsureCanDecline(recipient);
        pending.EnsureCanWithdraw(proposer);

        pending.Accept(recipient);
        Assert.Throws<InvalidOperationException>(() => pending.EnsureCanRemove(otherProfile));
        pending.EnsureCanRemove(proposer);
        pending.EnsureCanRemove(recipient);
    }
}
