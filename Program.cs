using System.CommandLine;
using System.Text.Json;
using JsonToDockerVars.Commands;
using JsonToDockerVars.Enums;
using JsonToDockerVars.Extensions;

namespace JsonToDockerVars;

internal static class Program
{
    private static class Descriptions
    {
        public static string RootDescription = "JSON to docker/koyeb converter";
        public static string OutputPathOptionDescription = "Output path. If not specified, will write to console. WILL OVERRIDE ANY CONTENT IN PROVIDED FILE";
        public static string FilePathOptionDescription = "Path to JSON file";
        public static string OutputFormatOptionDescription = "Do not use docker_file if multi line values are present. Use docker_string instead";

        public static string ExcludeOptionDescription = "Sections/SubSections/Values to exclude. Example:" +
                                                        "\n\tSection - will exclude whole section" +
                                                        "\n\tSection__SubSection - will exclude only sub section" +
                                                        "\n\tSection__SubSection__Value - will exclude only value" +
                                                        "\nMultiple excludes are available in format -e \"first\" \"seconds\" \"third\"";
    }


    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand(Descriptions.RootDescription);


        var filePathArg = new Argument<FileInfo>("filePath", Descriptions.FilePathOptionDescription).LegalFilePathsOnly().ExistingOnly();
        
        var outputFormatOption = new Option<OutputFormat>(["--output-format", "-of"], Descriptions.OutputFormatOptionDescription).FromAmong(Enum.GetNames<OutputFormat>());
        outputFormatOption.SetDefaultValue(OutputFormat.json);
        outputFormatOption.IsRequired = true;

        var outputPathOption = new Option<string>(["--output-path", "-op"], Descriptions.OutputPathOptionDescription);
        var excludeOption = new Option<IEnumerable<string>>(["--except", "-e",], Descriptions.ExcludeOptionDescription)
        {
            AllowMultipleArgumentsPerToken = true
        };

        var getVariablesCommand = VariableCommands.GetVariablesCommand(filePathArg, outputFormatOption, outputPathOption, excludeOption);
        getVariablesCommand.AddOption(outputPathOption);
        getVariablesCommand.AddOption(outputFormatOption);
        getVariablesCommand.AddOption(excludeOption);


        rootCommand.AddArgument(filePathArg);

        rootCommand.AddCommand(getVariablesCommand);
        rootCommand.AddCommand(SectionCommands.GetMainSectionsCommand(filePathArg));


        return await rootCommand.InvokeAsync(args);
    }
}