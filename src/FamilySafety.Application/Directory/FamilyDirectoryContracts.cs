namespace FamilySafety.Application.Directory;

public sealed record ActiveFamilyGroupView(Guid FamilyGroupId, string Name);

public sealed record FamilyDirectoryEntry(Guid ProfileId, string DisplayName);

public sealed record FamilyDirectoryMemberData(Guid ProfileId, string DisplayName);
