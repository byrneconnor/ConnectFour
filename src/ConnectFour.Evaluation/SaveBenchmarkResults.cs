using System.Text;

namespace ConnectFour.Evaluation
{
    // Writes BenchmarkResult results to csv
    public static class SaveBenchmarkResults
    {
        public static void Save(BenchmarkResult result, string outputDir)
        {
            // Get the player name for the file prefix
            StringBuilder playerName = new StringBuilder(result.PlayerName.Length);
            foreach (char ch in result.PlayerName)
            {
                // Replace any non-digit or non-letter with '-'
                playerName.Append(char.IsLetterOrDigit(ch) ? ch : '-');
            }
            
            // Create prefix based on player and split
            string filePrefix = $"{playerName}-{result.Split}";

            // Per-move rows as CSV.
            File.WriteAllText(Path.Combine(outputDir, filePrefix + "-full-results.csv"), BuildCsv(result));

            // Per-stage summary as JSON
            var summary = new
            {
                result.PlayerName,
                result.Split,
                result.Seeds,
                result.StageAggregate,
            };
            JsonHelpers.Save(Path.Combine(outputDir, filePrefix + "-group-aggregates.json"), summary);
        }

        private static string BuildCsv(BenchmarkResult result)
        {
            StringBuilder sb = new StringBuilder();

            // Set up the csv headers 
            sb.Append(CsvHelpers.Row("positionNumber", "stage", "seed", "chosenColumn", "illegal", "agreement", "regret", "resultPreserved", "speedRegret", "decisionMs", "nodes")).Append('\n');

            // Loop through and add to the csv
            foreach (MoveResult m in result.Moves)
            {
                sb.Append(CsvHelpers.Row(m.PositionNumber, m.Stage, m.Seed, m.ChosenColumn, m.Illegal, m.Agreement, m.Regret, m.ResultPreserved, m.SpeedRegret, m.DecisionMs, m.Nodes)).Append('\n');
            }

            return sb.ToString();
        }
    }
}