using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cookr.Api.Shared;

public enum ResultStatus
{
    Success,
    Invalid,
    NotFound,
    Conflict
}

public record Error(string Code, string Message);

public sealed class Result
{
    private Result(ResultStatus status, Error? error)
    {
        Status = status;
        Error = error;
    }

    public ResultStatus Status { get; }
    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result Success() => new(ResultStatus.Success, null);
    public static Result Invalid(Error error) => new(ResultStatus.Invalid, error);
    public static Result NotFound(Error error) => new(ResultStatus.NotFound, error);
    public static Result Conflict(Error error) => new(ResultStatus.Conflict, error);
}

public sealed class Result<T>
{
    private Result(ResultStatus status, T? value, Error? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public ResultStatus Status { get; }
    public T? Value { get; }
    public Error? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(ResultStatus.Success, value, null);
    public static Result<T> Invalid(Error error) => new(ResultStatus.Invalid, default, error);
    public static Result<T> NotFound(Error error) => new(ResultStatus.NotFound, default, error);
    public static Result<T> Conflict(Error error) => new(ResultStatus.Conflict, default, error);
}

public static class ResultExtensions
{
    public static ProblemHttpResult ToProblem(this Result result) =>
        Problem(result.Status, result.Error ?? throw new InvalidOperationException("Result has no error"));

    public static ProblemHttpResult ToProblem<T>(this Result<T> result) =>
        Problem(result.Status, result.Error ?? throw new InvalidOperationException("Result has no error"));

    private static ProblemHttpResult Problem(ResultStatus status, Error error) =>
        TypedResults.Problem(
            detail: error.Message,
            statusCode: status switch
            {
                ResultStatus.Invalid => StatusCodes.Status400BadRequest,
                ResultStatus.NotFound => StatusCodes.Status404NotFound,
                ResultStatus.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            },
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
