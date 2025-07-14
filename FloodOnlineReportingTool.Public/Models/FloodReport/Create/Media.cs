using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Create;

public class Media
{
    [GdsFieldErrorClass(GdsFieldTypes.FileUpload)]
    public IList<MediaItem> UploadedFiles { get; set; } = [];
}
