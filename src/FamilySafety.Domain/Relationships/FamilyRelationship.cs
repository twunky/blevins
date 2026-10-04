namespace FamilySafety.Domain.Relationships;

public sealed class FamilyRelationship
{
    private FamilyRelationship()
    {
    }

    public FamilyRelationship(
        Guid id,
        Guid familyGroupId,
        Guid proposerProfileId,
        Guid recipientProfileId,
        FamilyRelationshipLabel proposerLabel)
    {
        if (id == Guid.Empty || familyGroupId == Guid.Empty
            || proposerProfileId == Guid.Empty || recipientProfileId == Guid.Empty)
        {
            throw new ArgumentException("Relationship identifiers are required.");
        }

        if (proposerProfileId == recipientProfileId)
        {
            throw new ArgumentException("A person cannot propose a relationship with themselves.");
        }

        if (!Enum.IsDefined(proposerLabel))
        {
            throw new ArgumentOutOfRangeException(nameof(proposerLabel));
        }

        Id = id;
        FamilyGroupId = familyGroupId;
        ProposerProfileId = proposerProfileId;
        RecipientProfileId = recipientProfileId;
        ProposerLabel = proposerLabel;
        Status = FamilyRelationshipStatus.Pending;

        if (proposerProfileId.CompareTo(recipientProfileId) < 0)
        {
            FirstProfileId = proposerProfileId;
            SecondProfileId = recipientProfileId;
        }
        else
        {
            FirstProfileId = recipientProfileId;
            SecondProfileId = proposerProfileId;
        }
    }

    public Guid Id { get; private set; }

    public Guid FamilyGroupId { get; private set; }

    public Guid FirstProfileId { get; private set; }

    public Guid SecondProfileId { get; private set; }

    public Guid ProposerProfileId { get; private set; }

    public Guid RecipientProfileId { get; private set; }

    public FamilyRelationshipLabel ProposerLabel { get; private set; }

    public FamilyRelationshipStatus Status { get; private set; }

    public void Accept(Guid acceptingProfileId)
    {
        if (Status != FamilyRelationshipStatus.Pending)
        {
            throw new InvalidOperationException("Only a pending relationship proposal can be accepted.");
        }

        if (acceptingProfileId != RecipientProfileId)
        {
            throw new InvalidOperationException("Only the recipient can accept a relationship proposal.");
        }

        Status = FamilyRelationshipStatus.Accepted;
    }

    public void EnsureCanDecline(Guid decliningProfileId)
    {
        EnsurePendingRecipient(decliningProfileId);
    }

    public void EnsureCanWithdraw(Guid withdrawingProfileId)
    {
        if (Status != FamilyRelationshipStatus.Pending || withdrawingProfileId != ProposerProfileId)
        {
            throw new InvalidOperationException("Only the proposer can withdraw a pending relationship proposal.");
        }
    }

    public void EnsureCanRemove(Guid removingProfileId)
    {
        if (Status != FamilyRelationshipStatus.Accepted
            || (removingProfileId != ProposerProfileId && removingProfileId != RecipientProfileId))
        {
            throw new InvalidOperationException("Only a participant can remove an accepted relationship.");
        }
    }

    private void EnsurePendingRecipient(Guid profileId)
    {
        if (Status != FamilyRelationshipStatus.Pending || profileId != RecipientProfileId)
        {
            throw new InvalidOperationException("Only the recipient can decline a pending relationship proposal.");
        }
    }

    public static FamilyRelationshipLabel ReciprocalOf(FamilyRelationshipLabel label) => label switch
    {
        FamilyRelationshipLabel.Parent => FamilyRelationshipLabel.AdultChild,
        FamilyRelationshipLabel.AdultChild => FamilyRelationshipLabel.Parent,
        FamilyRelationshipLabel.SpousePartner => FamilyRelationshipLabel.SpousePartner,
        FamilyRelationshipLabel.Sibling => FamilyRelationshipLabel.Sibling,
        FamilyRelationshipLabel.OtherFamily => FamilyRelationshipLabel.OtherFamily,
        _ => throw new ArgumentOutOfRangeException(nameof(label)),
    };
}
