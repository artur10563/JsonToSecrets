using System.CommandLine;
using System.Text.Json;

namespace JsonToDockerVars;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("JSON to docker/koyeb converter");


        var filePath = new Argument<string>("filePath", "Path to JSON file");
        var outputFormat = new Option<string>("--output-format", "Output format")
            .FromAmong(["json", "docker", "koyeb"]);
        outputFormat.SetDefaultValue("json");
        outputFormat.IsRequired = true;

        var outputPath = new Option<string>(["--output-path", "-o"], "Output path. If not specified, will write to console. WILL OVERRIDE ANY CONTENT IN PROVIDED FILE");


        rootCommand.AddOption(outputFormat);
        rootCommand.AddGlobalOption(outputPath);

        var getVariablesCommand = GetVariablesCommand(filePath, outputFormat, outputPath);
        getVariablesCommand.AddOption(outputFormat);

        rootCommand.AddArgument(filePath);


        rootCommand.AddCommand(getVariablesCommand);
        rootCommand.AddCommand(GetMainSectionsCommand(filePath));

        return await rootCommand.InvokeAsync(args);
    }

    private static IEnumerable<string> EnumerateSingleProp(JsonProperty jsonProperty, string format)
    {
        return Service.Extract(jsonProperty).Select(jsonVar => format switch
        {
            "json" => jsonVar.ToJsonString(),
            "docker" => jsonVar.ToDockerString(),
            "koyeb" => jsonVar.ToKoyebString(),
            _ => jsonVar.ToJsonString()
        });
    }

    private static Command GetVariablesCommand(Argument<string> filePath, Option<string> outputFormat, Option<string> outputPath)
    {
        var enumerateSectionsCommand = new Command("variables", "Get variables from JSON file");
        enumerateSectionsCommand.SetHandler((jsonFile, format, path) =>
        {
            if (!File.Exists(jsonFile))
            {
                Console.WriteLine($"File {jsonFile} does not exist");
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
                var jsonContent = File.ReadAllText(jsonFile);
                using var doc = JsonDocument.Parse(jsonContent);

                foreach (var output in doc.RootElement.EnumerateObject()
                             .SelectMany(prop =>
                                 EnumerateSingleProp(prop, format)))
                {
                    (fileWriter ?? consoleWriter).Invoke(output);
                }

                if (fileWriter != null)
                {
                    Console.Write($"Output can be found at {Path.GetFullPath(path)}");
                }
            }
        }, filePath, outputFormat, outputPath);
        return enumerateSectionsCommand;
    }

    //TODO: ADD EXCEPT, ONLY SECTIONS FOR EXPORT.

    private static Command GetMainSectionsCommand(Argument<string> filePath)
    {
        var getMainSectionsCommand = new Command("sections", "Get main sections of JSON file");
        getMainSectionsCommand.SetHandler((jsonFile) =>
        {
            if (File.Exists(jsonFile))
            {
                var jsonContent = File.ReadAllText(jsonFile);
                using var doc = JsonDocument.Parse(jsonContent);

                foreach (var section in doc.RootElement.EnumerateObject())
                {
                    Console.WriteLine(section.Name);
                    //Does not display nested childer. TODO: display it in folder-like structure
                }
            }
            else
            {
                Console.WriteLine($"File {jsonFile} does not exist");
            }
        }, filePath);
        return getMainSectionsCommand;
    }
}