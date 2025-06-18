using Asp.Versioning;
using System.Globalization;

namespace FloodOnlineReportingTool.Public.Models.OpenApi;

internal record VersionInfo(ApiVersion Version)
{
    internal string DocumentName => string.Format(CultureInfo.CurrentCulture, "v{0}", Version.MajorVersion);
}
