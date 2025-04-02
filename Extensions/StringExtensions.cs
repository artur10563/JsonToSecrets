namespace JsonToDockerVars.Extensions;

public static class StringExtensions
{
    public static string NoEscaping(this string str) => str.Replace("\n", @"\n").Replace("\t", @"\t");
    public static string RemoveEscaping(this string str) => str.Replace("\n", @"").Replace("\t", @"");
}