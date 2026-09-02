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
            // Set up the csv headers 
            StringBuilder sb = new StringBuilder();
            sb.Append("positionNumber,stage,seed,chosenColumn,illegal,agreement,regret,resultPreserved,speedRegret,decisionMs,nodes\n");

            // Loop through and add to the csv
            foreach (MoveResult m in result.Moves)
            {
                sb.Append(m.PositionNumber).Append(',');
                sb.Append(m.Stage).Append(',');
                sb.Append(m.Seed).Append(',');
                sb.Append(m.ChosenColumn).Append(',');
                sb.Append(m.Illegal).Append(',');
                sb.Append(m.Agreement).Append(',');
                sb.Append(m.Regret).Append(',');
                sb.Append(m.ResultPreserved).Append(',');
                sb.Append(m.SpeedRegret).Append(',');
                sb.Append(m.DecisionMs).Append(',');
                sb.Append(m.Nodes).Append('\n');
            }

            return sb.ToString();
        }
    }
}