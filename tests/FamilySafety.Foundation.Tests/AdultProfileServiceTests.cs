using FamilySafety.Application.Profiles;
using FamilySafety.Domain.Profiles;

namespace FamilySafety.Foundation.Tests;

public sealed class AdultProfileServiceTests
{
    private static readonly Guid OwnerAccountId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid ProfileId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid FamilyGroupId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    [Fact]
    public void OptionalFieldsDefaultToOnlyMe()
    {
        var profile = new AdultProfile(ProfileId, OwnerAccountId, "Owner");

        Assert.Equal(ProfileFieldVisibility.OnlyMe, profile.ContactEmailVisibility);
        Assert.Equal(ProfileFieldVisibility.OnlyMe, profile.ContactPhoneVisibility);
        Assert.Equal(ProfileFieldVisibility.OnlyMe, profile.BirthdayVisibility);
    }

    [Fact]
    public async Task OwnerCanUpdateOnlyTheApprovedFields()
    {
        var repository = BuildRepository();
        var service = BuildService(OwnerAccountId, repository);

        var result = await service.UpdateCurrentAsync(
            new UpdateAdultProfileRequest(
                "  Alex Family  ",
                "  alex@example.com ",
                ProfileFieldVisibility.ActiveFamilyGroup,
                " (555) 010-0100 ",
                ProfileFieldVisibility.OnlyMe,
                2,
                29,
                ProfileFieldVisibility.ActiveFamilyGroup),
            CancellationToken.None);

        Assert.Equal("Alex Family", result.DisplayName);
        Assert.Equal("alex@example.com", result.ContactEmail);
        Assert.Equal("(555) 010-0100", result.ContactPhone);
        Assert.Equal((byte)2, result.BirthdayMonth);
        Assert.Equal((byte)29, result.BirthdayDay);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task ActiveSameGroupMemberReceivesOnlyExplicitlySharedOptionalFields()
    {
        var repository = BuildRepository();
        var viewerId = Guid.NewGuid();
        repository.ActiveMemberships.Add((viewerId, FamilyGroupId));
        var ownerService = BuildService(OwnerAccountId, repository);
        await ownerService.UpdateCurrentAsync(
            new UpdateAdultProfileRequest(
                "Owner",
                "shared@example.com",
                ProfileFieldVisibility.ActiveFamilyGroup,
                "private phone",
                ProfileFieldVisibility.OnlyMe,
                3,
                14,
                ProfileFieldVisibility.OnlyMe),
            CancellationToken.None);

        var viewerService = BuildService(viewerId, repository);
        var result = await viewerService.GetSharedAsync(ProfileId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Owner", result.DisplayName);
        Assert.Equal("shared@example.com", result.ContactEmail);
        Assert.Null(result.ContactPhone);
        Assert.Null(result.BirthdayMonth);
        Assert.Null(result.BirthdayDay);
    }

    [Fact]
    public async Task FamilyAdministratorReceivesNoVisibilityOverride()
    {
        var repository = BuildRepository();
        var administratorId = Guid.NewGuid();
        repository.ActiveMemberships.Add((administratorId, FamilyGroupId));
        repository.AdministratorAccounts.Add(administratorId);
        var ownerService = BuildService(OwnerAccountId, repository);
        await ownerService.UpdateCurrentAsync(
            new UpdateAdultProfileRequest(
                "Owner",
                "private@example.com",
                ProfileFieldVisibility.OnlyMe,
                null,
                ProfileFieldVisibility.OnlyMe,
                null,
                null,
                ProfileFieldVisibility.OnlyMe),
            CancellationToken.None);

        var administratorService = BuildService(administratorId, repository);
        var result = await administratorService.GetSharedAsync(ProfileId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Owner", result.DisplayName);
        Assert.Null(result.ContactEmail);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task UnrelatedInactiveAndOperatorAccountsReceiveNoProfile(
        bool inactiveFormerMember,
        bool applicationOperator)
    {
        var repository = BuildRepository();
        var viewerId = Guid.NewGuid();
        if (inactiveFormerMember)
        {
            repository.InactiveMemberships.Add((viewerId, FamilyGroupId));
        }

        var service = BuildService(viewerId, repository, applicationOperator);

        var result = await service.GetSharedAsync(ProfileId, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task InvalidBirthdayAndEmailReturnFieldSpecificErrors()
    {
        var service = BuildService(OwnerAccountId, BuildRepository());

        var exception = await Assert.ThrowsAsync<AdultProfileValidationException>(() =>
            service.UpdateCurrentAsync(
                new UpdateAdultProfileRequest(
                    "Owner",
                    "not an email",
                    ProfileFieldVisibility.OnlyMe,
                    null,
                    ProfileFieldVisibility.OnlyMe,
                    2,
                    30,
                    ProfileFieldVisibility.OnlyMe),
                CancellationToken.None));

        Assert.Contains(nameof(UpdateAdultProfileRequest.ContactEmail), exception.Errors.Keys);
        Assert.Contains(nameof(UpdateAdultProfileRequest.BirthdayDay), exception.Errors.Keys);
    }

    [Fact]
    public void SharedContractCannotExposeSignInIdentifierOrVisibilitySettings()
    {
        var properties = typeof(AdultProfileSharedView).GetProperties().Select(property => property.Name).ToArray();

        Assert.DoesNotContain("SignInIdentifier", properties);
        Assert.DoesNotContain("ContactEmailVisibility", properties);
        Assert.DoesNotContain("ContactPhoneVisibility", properties);
        Assert.DoesNotContain("BirthdayVisibility", properties);
    }

    private static AdultProfileService BuildService(
        Guid accountId,
        FakeAdultProfileRepository repository,
        bool isApplicationOperator = false) =>
        new(new FakeCurrentAccountAccessor(accountId, isApplicationOperator), repository);

    private static FakeAdultProfileRepository BuildRepository()
    {
        var repository = new FakeAdultProfileRepository();
        repository.Profiles.Add(new AdultProfile(ProfileId, OwnerAccountId, "Owner"));
        repository.ActiveMemberships.Add((OwnerAccountId, FamilyGroupId));
        return repository;
    }

    private sealed class FakeCurrentAccountAccessor(Guid accountId, bool isApplicationOperator)
        : ICurrentAccountAccessor
    {
        public CurrentAccount Current { get; } = new(accountId, isApplicationOperator);
    }

    private sealed class FakeAdultProfileRepository : IAdultProfileRepository
    {
        public List<AdultProfile> Profiles { get; } = [];

        public HashSet<(Guid AccountId, Guid GroupId)> ActiveMemberships { get; } = [];

        public HashSet<(Guid AccountId, Guid GroupId)> InactiveMemberships { get; } = [];

        public HashSet<Guid> AdministratorAccounts { get; } = [];

        public int SaveCount { get; private set; }

        public Task<AdultProfile?> FindByAccountIdAsync(Guid accountId, CancellationToken cancellationToken) =>
            Task.FromResult(Profiles.SingleOrDefault(profile => profile.AccountId == accountId));

        public Task<AdultProfile?> FindByIdAsync(Guid profileId, CancellationToken cancellationToken) =>
            Task.FromResult(Profiles.SingleOrDefault(profile => profile.Id == profileId));

        public Task<bool> AreActiveMembersOfSameGroupAsync(
            Guid requestingAccountId,
            Guid targetAccountId,
            CancellationToken cancellationToken)
        {
            var requestingGroups = ActiveMemberships
                .Where(membership => membership.AccountId == requestingAccountId)
                .Select(membership => membership.GroupId)
                .ToHashSet();
            var result = ActiveMemberships.Any(
                membership => membership.AccountId == targetAccountId && requestingGroups.Contains(membership.GroupId));
            return Task.FromResult(result);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
