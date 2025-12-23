namespace FloodOnlineReportingTool.Database.Models.ResultModels;

public record DeleteResult<T>(bool IsSuccess, ICollection<string> Errors)
{
    internal static DeleteResult<T> Success() => new(IsSuccess: true, []);

    internal static DeleteResult<T> Failure(ICollection<string> errors) => new(IsSuccess: false, errors);
}
