using Microsoft.AspNetCore.Components.Forms;
namespace FloodOnlineReportingTool.Public.Models.Media;

public readonly record struct RejectedFile(IBrowserFile File, FileRejectionReason FileRejectionReason);
