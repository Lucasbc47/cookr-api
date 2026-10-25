using System.Globalization;
using System.Text.Json;
using FluentValidation;

namespace Cookr.Api.Shared;

public static class ErrorCodes
{
    public const string ValidationFailed = "validation_failed";
    public const string BadRequest = "bad_request";
    public const string NotFound = "not_found";
    public const string MethodNotAllowed = "method_not_allowed";
    public const string InternalError = "internal_error";
    public const string Unknown = "error";
}

public static class ErrorHandling
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.InvariantCulture;
        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) =>
            member is null ? null : JsonNamingPolicy.CamelCase.ConvertName(member.Name);

        return services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions.TryAdd("code", context.ProblemDetails.Status switch
                {
                    StatusCodes.Status400BadRequest => ErrorCodes.BadRequest,
                    StatusCodes.Status404NotFound => ErrorCodes.NotFound,
                    StatusCodes.Status405MethodNotAllowed => ErrorCodes.MethodNotAllowed,
                    StatusCodes.Status500InternalServerError => ErrorCodes.InternalError,
                    _ => ErrorCodes.Unknown
                }));
    }

    public static WebApplication UseErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            StatusCodeSelector = ex => ex is BadHttpRequestException badRequest
                ? badRequest.StatusCode
                : StatusCodes.Status500InternalServerError
        });
        app.UseStatusCodePages();
        return app;
    }
}
