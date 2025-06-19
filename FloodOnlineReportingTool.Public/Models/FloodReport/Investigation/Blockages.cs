using GdsBlazorComponents;

namespace FloodOnlineReportingTool.Public.Models.FloodReport.Investigation;

public class Blockages
{
    [GdsFieldErrorClass(GdsFieldTypes.Radio)]
    public Guid? HasKnownProblemsId { get; set; }

    [GdsFieldErrorClass(GdsFieldTypes.Textarea)]
    public string? KnownProblemDetails { get; set; }
}
