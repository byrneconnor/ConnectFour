using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConnectFour.Evaluation
{
    // Helpers with json data
    public static class JsonHelpers
    {
        // Set up JSON configurations
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            // JSON keys in camelCase
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Indents JSON lines, easier to read
            WriteIndented = true,
            // write enums as their names rather than numbers - used for Disc
            Converters = { new JsonStringEnumConverter() },
        };

        // Read in JSON data
        public static List<SolvedPosition> Read(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<SolvedPosition>>(json, JsonOptions)
                ?? throw new Exception($"Could not deserialize JSON at {path}.");
        }

        // Save JSON data
        public static void Save(string path, List<SolvedPosition> positions)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(positions, JsonOptions));
        }
    }
}