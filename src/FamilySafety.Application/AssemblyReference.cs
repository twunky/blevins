using System.Reflection;

namespace FamilySafety.Application;

/// <summary>
/// Provides a stable reference to the Application assembly for architecture checks.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
