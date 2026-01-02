using System.CommandLine;
using System.Text.Json;
using JsonToDockerVars.Enums;
using JsonToDockerVars.Extensions;

namespace JsonToDockerVars.Commands;

public static class VariableCommands
{
    public static Command GetVariablesCommand(Argument<FileInfo> filePath, Option<OutputFormat> outputFormat, Option<string> outputPath, Option<IEnumerable<string>> exclude)
    {
        var enumerateSectionsCommand = new Command("variables", "Get variables from JSON file");
        enumerateSectionsCommand.SetHandler((jsonFile, format, path, except) =>
        {
            var validationResult = jsonFile.IsValidJson();
            if (!validationResult.IsValid)
            {
                Console.WriteLine(validationResult.ErrorMessage);
                enumerateSectionsCommand.Invoke("--help");
                return;
            }

            Action<string> consoleWriter = format switch
            {
                OutputFormat.docker_string => Console.Write,
                _ => Console.WriteLine
            };

            Action<string>? fileWriter = null;
            StreamWriter? streamWriter = null;
            if (!string.IsNullOrEmpty(path))
            {
                streamWriter = new StreamWriter(path, append: false);
                fileWriter = format switch
                {
                    OutputFormat.docker_string => streamWriter.Write,
                    _ => streamWriter.WriteLine
                };
            }

            using (streamWriter)
            {
                using var doc = JsonDocument.Parse(jsonFile.ReadAllText());


                foreach (var output in EnumerateAllProps(doc, format, except))
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

    private static IEnumerable<string> EnumerateAllProps(JsonDocument document, OutputFormat format, IEnumerable<string> exclude)
    {
        return Service.ExtractAll(document, exclude)
            .Select(jsonVar => FormatVariable(jsonVar, format));
    }

    private static IEnumerable<string> EnumerateSingleProp(JsonProperty jsonProperty, OutputFormat format, IEnumerable<string> exclude)
    {
        return Service.Extract(jsonProperty, exclude)
            .Select(jsonVar => FormatVariable(jsonVar, format));
    }

    private static string FormatVariable(JsonVariable jsonVar, OutputFormat format) =>
        format switch
        {
            OutputFormat.json => jsonVar.ToJsonString(),
            OutputFormat.docker_string => jsonVar.ToDockerString(),
            OutputFormat.docker_file => jsonVar.ToDockerEnvFileLineString(),
            OutputFormat.koyeb => jsonVar.ToKoyebString(),
            _ => jsonVar.ToJsonString()
        };
}