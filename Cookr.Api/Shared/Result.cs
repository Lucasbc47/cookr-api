using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cookr.Api.Shared;

public enum ResultStatus
{
    Success,
    NotFound,
    Conflict
}

public record Result(ResultStatus Status, string? Error = null)
{
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success() => new(ResultStatus.Success);
    public static Result NotFound(string error) => new(ResultStatus.NotFound, error);
    public static Result Conflict(string error) => new(ResultStatus.Conflict, error);
}

public record Result<T>(ResultStatus Status, T? Value = default, string? Error = null)
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result<T> Success(T value) => new(ResultStatus.Success, value);
    public static Result<T> NotFound(string error) => new(ResultStatus.NotFound, Error: error);
    public static Result<T> Conflict(string error) => new(ResultStatus.Conflict, Error: error);
}

public static class ResultExtensions
{
    public static ProblemHttpResult ToProblem(this Result result) => Problem(result.Status, result.Error);

    public static ProblemHttpResult ToProblem<T>(this Result<T> result) => Problem(result.Status, result.Error);

    private static ProblemHttpResult Problem(ResultStatus status, string? error) =>
        TypedResults.Problem(detail: error, statusCode: status switch
        {
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            ResultStatus.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        });
}
