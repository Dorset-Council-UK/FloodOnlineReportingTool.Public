using System.Diagnostics.CodeAnalysis;

namespace FloodOnlineReportingTool.Database.Models.ResultModels;

public record Result<T>(bool IsSuccess, T? Value, ICollection<string> Errors)
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; init; } = IsSuccess;

    internal static Result<T> Success(T value) => new(IsSuccess: true, value, []);

    internal static Result<T> Failure(ICollection<string> errors) => new(IsSuccess: false, Value: default, errors);
}
