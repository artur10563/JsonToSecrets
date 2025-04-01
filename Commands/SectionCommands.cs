using System.CommandLine;
using System.Text.Json;
using JsonToDockerVars.Extensions;

namespace JsonToDockerVars.Commands;

public static class SectionCommands
{
    public static Command GetMainSectionsCommand(Argument<FileInfo> filePath)
    {
        var getMainSectionsCommand = new Command("sections", "Get main sections of JSON file");
        getMainSectionsCommand.SetHandler((jsonFile) =>
        {
            var validationResult = jsonFile.IsValidJson();
            if (!validationResult.IsValid)
            {
                Console.WriteLine(validationResult.ErrorMessage);
                return;
            }


            using var doc = JsonDocument.Parse(jsonFile.ReadAllText());

            foreach (var section in doc.RootElement.EnumerateObject())
            {
                Console.WriteLine(section.Name);
                //Does not display nested childer. TODO: display it in folder-like structure
            }
        }, filePath);
        return getMainSectionsCommand;
    }
}