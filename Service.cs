using System.Text.Json;
using JsonToDockerVars.Extensions;

namespace JsonToDockerVars;

public sealed record JsonVariable(string Name, string Value)
{
    public string ToJsonString() => @$"{this.Name.NoEscaping()}:{this.Value.NoEscaping()}";
    public string ToDockerString() => @$"-e {this.Name.NoEscaping()}=""{this.Value.RemoveEscaping()}"" "; //Need to remove newlines and tabs for multi line values
    public string ToDockerEnvFileLineString() => $@"{this.Name.NoEscaping()}=""{this.Value.RemoveEscaping()}"""; 
    public string ToKoyebString() => $@"{this.Name.NoEscaping()}=""{this.Value}"""; //Koyeb value must be escaped!
};

public static class Service
{
    /// <summary>
    /// Entry point for a single JsonProperty
    /// </summary>
    public static IEnumerable<JsonVariable> Extract(JsonProperty property, IEnumerable<string>? exclude = null)
    {
        var excludeHS = exclude != null
            ? [..exclude]
            : new HashSet<string>();
        return Traverse(property.Value, property.Name, excludeHS);
    }

    /// <summary>
    /// Entry point for entire JsonDocument
    /// </summary>
    public static IEnumerable<JsonVariable> ExtractAll(JsonDocument doc, IEnumerable<string>? exclude = null)
    {
        var excludeHS = exclude != null ? new HashSet<string>(exclude) : new HashSet<string>();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            foreach (var v in Traverse(prop.Value, prop.Name, excludeHS))
                yield return v;
        }
    }

    /// <summary>
    /// Core recursive traversal engine
    /// Handles objects, arrays, nested arrays, and primitives
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static IEnumerable<JsonVariable> Traverse(JsonElement element, string currentPath, HashSet<string> exclude)
    {
        if (exclude.Contains(currentPath))
            yield break;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var nextPath = string.IsNullOrEmpty(currentPath) ? prop.Name : $"{currentPath}__{prop.Name}";
                    foreach (var v in Traverse(prop.Value, nextPath, exclude))
                        yield return v;
                }
                break;

            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var nextPath = $"{currentPath}__{index}";
                    foreach (var v in Traverse(item, nextPath, exclude))
                        yield return v;
                    index++;
                }
                break;

            case JsonValueKind.Undefined:
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                yield return new JsonVariable(currentPath, element.ValueKind == JsonValueKind.Null ? "" : element.ToString());
                break;
            
            default: throw new ArgumentOutOfRangeException(nameof(element), "Unexpected JsonValueKind");
        }
    }
}