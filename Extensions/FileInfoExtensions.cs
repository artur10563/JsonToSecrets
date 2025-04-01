using JsonToDockerVars.Constants;

namespace JsonToDockerVars.Extensions;

public static class FileInfoExtensions
{
    public static string ReadAllText(this FileInfo file)
    {
        if (!file.Exists) throw new FileNotFoundException("File not found");

        using var reader = file.OpenText();
        return reader.ReadToEnd();
    }

    public static (bool IsValid, string ErrorMessage) IsValidJson(this FileInfo file)
    {
        if (!file.Exists)
            return (false, ErrorMessages.FileErrors.DoesNotExists);

        if (!file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            return (false, ErrorMessages.FileErrors.InvalidExtension);

        return (true, string.Empty);
    }
}