namespace FamilySafety.Domain.Profiles;

public sealed class Account
{
    private Account()
    {
    }

    public Account(Guid id, string signInIdentifier)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An account ID is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(signInIdentifier);

        Id = id;
        SignInIdentifier = signInIdentifier.Trim();
    }

    public Guid Id { get; private set; }

    public string SignInIdentifier { get; private set; } = string.Empty;
}
