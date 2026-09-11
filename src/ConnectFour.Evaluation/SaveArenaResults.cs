using System.Text;

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
            sb.Append(CsvHelpers.Row("playerOne", "playerTwo", "playerOneSeed", "playerTwoSeed","winner", "totalMoves", "playerOneMs", "playerTwoMs")).Append('\n');

            // One row per game
            foreach (GameResult game in result.Games)
            {
                sb.Append(CsvHelpers.Row(game.PlayerOne, game.PlayerTwo, game.PlayerOneSeed, game.PlayerTwoSeed, game.Winner, game.TotalMoves, game.PlayerOneMs, game.PlayerTwoMs)).Append('\n');
            }

            File.WriteAllText(csvPath, sb.ToString());
        }
    }
}