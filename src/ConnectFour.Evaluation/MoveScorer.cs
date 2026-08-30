namespace ConnectFour.Evaluation
{
    // USed to score moves by agents against the solvers benchmark positions
    public sealed record MoveScore(
        bool Agreement, // chosen move is optimal, (GetMove is in bestColumns)
        int Regret, // the value the player gave up with a bad move (bestScore minus value of the column chosen)               
        bool ResultPreserved, // chosen move still inline with best achievable result
        int? SpeedRegret // equals regret if result is preserved
        );

    public static class MoveScorer
    {
        public static MoveScore Score(SolvedPosition position, int chosenColumn)
        {
            // Check chosenColumn within column range
            if (chosenColumn < 0 || chosenColumn > 6)
            {
                throw new Exception("chosenColumn outside range [0-6]");
            }

            // Store column scores
            int?[] columnScores = position.ColumnScores;

            // Store scores from chosen column
            int? chosenScore = columnScores[chosenColumn];

            // Raise error if chosenScore is null
            if (chosenScore is null)
            {
                throw new Exception($"Chosen column {chosenColumn} is full for {position.Position}");
            }

            // Convert data to int
            int score = chosenScore.Value;
            int value = position.Value;

            // Calcualte agreement, regret and if result preserves
            bool agreement = position.BestColumns.Contains(chosenColumn);
            int regret = value - score;
            bool outcomePreserved = Math.Sign(score) == Math.Sign(value);
            int? speedRegret;
            if (outcomePreserved)
            {
                speedRegret = regret;
            }
            else
            {
                speedRegret = null;
            }

            return new MoveScore(agreement, regret, outcomePreserved, speedRegret);

        }
    }

}