using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cookr.Api.Shared;

public static class JsonConfig
{
    public static void Apply(JsonSerializerOptions options)
    {
        options.NumberHandling = JsonNumberHandling.Strict;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }
}
