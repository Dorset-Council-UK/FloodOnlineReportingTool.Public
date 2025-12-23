namespace FloodOnlineReportingTool.Database.Models.ResultModels;

public record CreateOrUpdateResult<T>(bool IsSuccess, T? ResultModel, ICollection<string> Errors)
{
    internal static CreateOrUpdateResult<T> Success(T resultModel) => new(IsSuccess: true, resultModel, []);

    internal static CreateOrUpdateResult<T> Failure(ICollection<string> errors) => new(IsSuccess: false, ResultModel: default, errors);
}

