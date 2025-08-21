namespace FloodOnlineReportingTool.Database.Models;

public static class FloodImpactExtensions
{
    public static bool IsInternal(this FloodImpact? floodImpact)
    {
        if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
        {
            return false;
        }

        return floodImpact.CategoryPriority.Equals(FloodImpactPriority.Internal, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsExternal(this FloodImpact? floodImpact)
    {
        if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
        {
            return false;
        }

        return floodImpact.CategoryPriority.Equals(FloodImpactPriority.External, StringComparison.OrdinalIgnoreCase);
    }
}
