using System.Reflection;

namespace FamilySafety.Domain;

/// <summary>
/// Provides a stable reference to the Domain assembly for architecture checks.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
