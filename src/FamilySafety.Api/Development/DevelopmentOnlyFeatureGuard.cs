namespace FamilySafety.Api.Development;

public static class DevelopmentOnlyFeatureGuard
{
    public static void EnsureAllowed(bool isDevelopment, bool enabled, string featureName)
    {
        if (enabled && !isDevelopment)
        {
            throw new InvalidOperationException($"{featureName} can be enabled only in Development.");
        }
    }
}
