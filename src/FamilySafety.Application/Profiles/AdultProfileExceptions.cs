namespace FamilySafety.Application.Profiles;

public sealed class CurrentAccountUnavailableException : Exception
{
    public CurrentAccountUnavailableException()
        : base("A current account is required.")
    {
    }
}

public sealed class AdultProfileNotFoundException : Exception
{
    public AdultProfileNotFoundException()
        : base("The adult profile was not found.")
    {
    }
}

public sealed class AdultProfileValidationException : Exception
{
    public AdultProfileValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("The adult profile request is invalid.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
