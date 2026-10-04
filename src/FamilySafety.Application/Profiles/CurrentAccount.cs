namespace FamilySafety.Application.Profiles;

public sealed record CurrentAccount(Guid AccountId, bool IsApplicationOperator = false);

public interface ICurrentAccountAccessor
{
    CurrentAccount? Current { get; }
}
