namespace FloodOnlineReportingTool.Database.Models.Flood;

public record FloodReportCreateOrUpdateResult(bool IsSuccess, FloodReport? FloodReport, ICollection<string> Errors)
{
    internal static FloodReportCreateOrUpdateResult Success(FloodReport floodReport) => new(IsSuccess: true, floodReport, []);

    internal static FloodReportCreateOrUpdateResult Failure(ICollection<string> errors) => new(IsSuccess: false, FloodReport: null, errors);
}

