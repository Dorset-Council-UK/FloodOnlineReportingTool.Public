namespace FloodOnlineReportingTool.Database.Models.ResultModels;

public record GetResult<T>(bool IsSuccess, T? ResultModel, ICollection<string> Errors)
{
    internal static GetResult<T> Success(T resultModel) => new(IsSuccess: true, resultModel, []);

    internal static GetResult<T> Failure(ICollection<string> errors) => new(IsSuccess: false, ResultModel: default, errors);
}
