using System.CommandLine;
using System.Text.Json;
using JsonToDockerVars.Extensions;

namespace JsonToDockerVars.Commands;

public static class VariableCommands
{
    public static Command GetVariablesCommand(Argument<FileInfo> filePath, Option<string> outputFormat, Option<string> outputPath, Option<IEnumerable<string>> exclude)
    {
        var enumerateSectionsCommand = new Command("variables", "Get variables from JSON file");
        enumerateSectionsCommand.SetHandler((jsonFile, format, path, except) =>
        {
            var validationResult = jsonFile.IsValidJson();
            if (!validationResult.IsValid)
            {
                Console.WriteLine(validationResult.ErrorMessage);
                return;
            }

            Action<string> consoleWriter = format switch
            {
                "docker" => Console.Write,
                _ => Console.WriteLine
            };

            Action<string>? fileWriter = null;
            StreamWriter? streamWriter = null;
            if (!string.IsNullOrEmpty(path))
            {
                streamWriter = new StreamWriter(path, append: false);
                fileWriter = format switch
                {
                    "docker" => streamWriter.Write,
                    _ => streamWriter.WriteLine
                };
            }

            using (streamWriter)
            {
                using var doc = JsonDocument.Parse(jsonFile.ReadAllText());

                foreach (var output in doc.RootElement.EnumerateObject()
                             .SelectMany(prop =>
                                 EnumerateSingleProp(prop, format, except)))
                {
                    (fileWriter ?? consoleWriter).Invoke(output);
                }

                if (fileWriter != null)
                {
                    Console.Write($"Output can be found at {Path.GetFullPath(path)}");
                }
            }
        }, filePath, outputFormat, outputPath, exclude);
        return enumerateSectionsCommand;
    }

    private static IEnumerable<string> EnumerateSingleProp(JsonProperty jsonProperty, string format, IEnumerable<string> exclude)
    {
        return Service.Extract(jsonProperty, exclude).Select(jsonVar => format switch
        {
            "json" => jsonVar.ToJsonString(),
            "docker" => jsonVar.ToDockerString(),
            "koyeb" => jsonVar.ToKoyebString(),
            _ => jsonVar.ToJsonString()
        });
    }
}