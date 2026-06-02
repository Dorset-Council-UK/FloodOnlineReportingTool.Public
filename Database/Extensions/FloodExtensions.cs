namespace FloodOnlineReportingTool.Database.Models.Flood;

internal static class FloodExtensions
{
    extension(FloodImpact? floodImpact)
    {
        internal bool IsInternal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
                {
                    return false;
                }

                return floodImpact.CategoryPriority.Equals(FloodImpactPriority.Internal, StringComparison.OrdinalIgnoreCase);
            }
        }

        internal bool IsExternal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(floodImpact?.CategoryPriority))
                {
                    return false;
                }

                return floodImpact.CategoryPriority.Equals(FloodImpactPriority.External, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
