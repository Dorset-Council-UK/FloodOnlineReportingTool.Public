#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FloodOnlineReportingTool.Database.Models.Flood;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class FloodImpactExtensions
{
    internal static bool IsInternal(this FloodImpact? floodImpact)
    {
        if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
        {
            return false;
        }

        return floodImpact.CategoryPriority.Equals(FloodImpactPriority.Internal, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool IsExternal(this FloodImpact? floodImpact)
    {
        if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
        {
            return false;
        }

        return floodImpact.CategoryPriority.Equals(FloodImpactPriority.External, StringComparison.OrdinalIgnoreCase);
    }
}
