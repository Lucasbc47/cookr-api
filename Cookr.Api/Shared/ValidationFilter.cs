using System.Text.Json;
using FluentValidation;

namespace Cookr.Api.Shared;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(context);

        var argument = context.Arguments.OfType<T>().First();
        var result = await validator.ValidateAsync(argument);
        if (result.IsValid) return await next(context);

        var errors = result.Errors
            .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return TypedResults.ValidationProblem(
            errors,
            extensions: new Dictionary<string, object?> { ["code"] = ErrorCodes.ValidationFailed });
    }
}
