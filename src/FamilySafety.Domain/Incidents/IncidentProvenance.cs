namespace FamilySafety.Domain.Incidents;

internal static class IncidentProvenance
{
    public static void EnsureSource(IncidentInformationCategory category, string? sourceDescription,
        bool sourceUnknown)
    {
        if (sourceUnknown == !string.IsNullOrWhiteSpace(sourceDescription))
            throw new ArgumentException("Provide a source description or explicitly mark the source unknown.");
        if (category == IncidentInformationCategory.Confirmed && sourceUnknown)
            throw new ArgumentException("Confirmed information must include its verification source.");
    }
}
