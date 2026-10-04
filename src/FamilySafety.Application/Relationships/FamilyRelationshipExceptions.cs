namespace FamilySafety.Application.Relationships;

public sealed class FamilyRelationshipUnavailableException : Exception
{
}

public sealed class FamilyRelationshipConflictException : Exception
{
}

public sealed class FamilyRelationshipValidationException : Exception
{
    public FamilyRelationshipValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("The relationship request is invalid.") => Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
