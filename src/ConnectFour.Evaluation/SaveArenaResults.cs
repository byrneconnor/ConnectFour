using System.Text;
using System.Text.Json;

namespace ConnectFour.Evaluation
{
    // Save Arena results
    public class SaveArenaResults
    {
        public static void Save(ArenaResult result, string dataFolder)
        {
            
            // File paths
            string csvPath = Path.Combine(dataFolder, "arena-full-results.csv");
            string jsonPath = Path.Combine(dataFolder, "arena-summary.json");

            // Write the per game CSV
            SaveGamesCsv(result, csvPath);

            // Write the summary JSON 
            var summary = new
            {
                result.GamesPerPairing,
                result.Seed,
                result.Pairings,
                result.Standings
            };
            
            // Save json
            JsonHelpers.Save(jsonPath, summary);
        }

        // Build and write the full per game CSV
        private static void SaveGamesCsv(ArenaResult result, string csvPath)
        {
            StringBuilder sb = new StringBuilder();

            // Header row
            sb.AppendLine("playerOne,playerTwo,playerOneSeed,playerTwoSeed,winner,totalMoves,playerOneMs,playerTwoMs");

            // One row per game
            foreach (GameResult game in result.Games)
            {
                sb.Append(game.PlayerOne);
                sb.Append(',');
                sb.Append(game.PlayerTwo);
                sb.Append(',');
                sb.Append(game.PlayerOneSeed.ToString());
                sb.Append(',');
                sb.Append(game.PlayerTwoSeed.ToString());
                sb.Append(',');
                sb.Append(game.Winner);
                sb.Append(',');
                sb.Append(game.TotalMoves.ToString());
                sb.Append(',');
                sb.Append(game.PlayerOneMs.ToString());
                sb.Append(',');
                sb.Append(game.PlayerTwoMs.ToString());
                sb.AppendLine();
            }

            File.WriteAllText(csvPath, sb.ToString());
        }
    }
}