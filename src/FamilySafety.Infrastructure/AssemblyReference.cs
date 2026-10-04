using System.Reflection;

namespace FamilySafety.Infrastructure;

/// <summary>
/// Provides a stable reference to the Infrastructure assembly for architecture checks.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
