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
    /// Enumerate JsonProperty
    /// </summary>
    /// <param name="property">JsonSection from which values are extracted</param>
    /// <param name="exclude">Sections/SubSections/Values to exclude</param>
    /// <returns>{Section__Sub_Section:value}</returns>
    public static IEnumerable<JsonVariable> Extract(JsonProperty property, IEnumerable<string>? exclude = null)
    {
        var excludeHS = exclude != null
            ? [..exclude]
            : new HashSet<string>();

        return InternalExtractAllValues(property, excludeHS);
    }

    /// <summary>
    /// Enumerate JsonProperty
    /// </summary>
    /// <param name="property">JsonSection from which values are extracted</param>
    /// <param name="exclude">Sections/SubSections/Values to exclude</param>
    /// <param name="prefix"> DO NOT PASS PREFIX AS IT IS BEING HANDLED INSIDE OF METHOD</param>
    /// <returns>{Section__Sub_Section:value}</returns>
    private static IEnumerable<JsonVariable> InternalExtractAllValues(JsonProperty property, HashSet<string> exclude, string prefix = "")
    {
        prefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}__";

        foreach (var sub in property.Value.EnumerateObject())
        {
            var prefixedName = $"{prefix}{property.Name}";
            if (exclude.Contains(property.Name) || exclude.Contains(prefixedName)) continue;

            //If child is section - continue extracting from it
            if (sub.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var value in InternalExtractAllValues(sub, exclude, prefixedName))
                    yield return value;
            }
            //Child is value, end extracting
            else
            {
                var name = $"{prefix}{property.Name}__{sub.Name}";

                if (exclude.Contains(name)) continue;

                yield return new JsonVariable(name, sub.Value.ToString());
            }
        }
    }
}