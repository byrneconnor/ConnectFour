using System.Diagnostics; // for timings

using ConnectFour.Core;

namespace ConnectFour.Evaluation
{
    // Builds a fresh player for one scoring run. Disc is side to move, seed is
    // the random seed set for reproducibility for stochastic players (MCTS)
    public delegate Player CreatePlayer(Disc disc, int seed);

    // Scores from an agent's decision for a certain benchmark position
    // Scores calculated by MoveScorer
    public sealed record MoveResult(
        int PositionNumber, // which position we're using
        Stage Stage, // Stage for position
        int Seed, // random seed used for that run
        int ChosenColumn, // column chosen by player
        bool Illegal, // flag if it was illegal idicating a bug in player code
        bool Agreement, // Does chosen move match benchmark?
        int Regret, // If they don't agrre, return regret score
        bool ResultPreserved, // If they don't agree, is the result preserved?
        int? SpeedRegret, // returns regret if result is preserved
        double DecisionMs, // time to make the move
        long? Nodes); // nodes explored to make decision, for minimax only

    // Aggregated results by stage
    public sealed record StageSummary(
        Stage Stage, // Stage for averages
        int Positions, // number of benchmark positions tested against
        int Moves, // number of moves made
        int IllegalMoves, // number of illegal moves made
        double AgreementRate, // average agreement rate
        double MeanRegret, // average regret
        double ResultPreservedRate, // average resultPrevserved rate
        double? MeanSpeedRegret, // average regret
        double MeanDecisionMs, // average decision time
        double P95DecisionMs, // 95 percentile decision time
        double MaxDecisionMs, // max time for a move
        double? MeanNodes); // average nodes explored

    // Combination of MoveResults and StageSummary
    public sealed record BenchmarkResult(
        string PlayerName, // player and configuration evaluated
        string Split, // train or test split
        List<int> Seeds, // seeds used for reproducibility
        List<StageSummary> StageAggregate, // aggregated per stage results
        List<MoveResult> Moves); // full results

    // Code to evaluate different agents/configurations against training and test benchmark data
    public static class BenchmarkEvaluation
    {
        // Run loop to gather results
        public static BenchmarkResult Run(
            string playerName, string splitLabel, CreatePlayer playerConfiguration, 
            List<SolvedPosition> positions, List<int> seeds) 
        {
            // Check positions and seeds are not empty
            if (positions.Count == 0)
                throw new ArgumentException("No positions to run.");
            if (seeds.Count == 0)
                throw new ArgumentException("Need at least one seed (use one seed for a deterministic player).");

            // Set up list to store every move
            List<MoveResult> data = new List<MoveResult>(positions.Count * seeds.Count);
            
            // Loop through each position to get score
            for (int i = 0; i < positions.Count; i++)
            {
                // Update message to console
                Console.WriteLine($"Position {i + 1}/{positions.Count}...");

                // Get results for individual position across multiple seeds
                List<MoveResult> positionRows = ScorePosition(i, positions[i], playerConfiguration, seeds);

                // Add each result for that position to full data
                foreach (MoveResult row in positionRows)
                {
                    data.Add(row);
                }
            }

            // Get aggregate data
            List<StageSummary> aggregatedData = Aggregate(data, seeds.Count);

            // Return aggregate and full data
            return new BenchmarkResult(playerName, splitLabel, seeds, aggregatedData, data);
        }

        // Method to score each position per seed
        private static List<MoveResult> ScorePosition(
            int positionNumber, SolvedPosition position, CreatePlayer playerConfiguration, List<int> seeds)
        {
            // Get stage and disc values
            Stage stage = SourceFile.GetStage(position.SourceFile);
            Disc disc = SolvedPosition.SideToMove(position.Position);

            // Set up results for individual position across multiple seeds
            List<MoveResult> positionRows = new List<MoveResult>(seeds.Count);

            // Lopp through each seed
            foreach (int seed in seeds)
            {
                // Set up positions on fresh board each time
                Board board = SolverBoard.SolverStringPositionToBoard(position.Position);

                // Set up player 
                Player player = playerConfiguration(disc, seed);

                // Time the move decision
                long startTime = Stopwatch.GetTimestamp();
                int column = player.GetMove(board);
                // Get decision time converted to ms
                double decisionMs = (Stopwatch.GetTimestamp() - startTime) * 1000.0 / Stopwatch.Frequency;

                // Get node count from player
                long? nodes = player.GetNodesSearched;

                // Record if a move is illegal without scoring it
                if (!board.IsValidMove(column))
                {
                    // Set illegal, agreement, regret, resultPreserved, speedRegret
                    positionRows.Add(new MoveResult(
                        positionNumber, stage, seed, column, Illegal: true,
                        Agreement: false, Regret: 0, ResultPreserved: false, SpeedRegret: null,
                        decisionMs, nodes));
                    continue;
                }
                else
                {
                    // Score legal moves and add to data
                    MoveScore score = MoveScorer.Score(position, column);
                    positionRows.Add(new MoveResult(
                        positionNumber, stage, seed, column, Illegal: false,
                        score.Agreement, score.Regret, score.ResultPreserved, score.SpeedRegret,
                        decisionMs, nodes));
                }

                // Update message to console
                Console.WriteLine($"  Seed {seed} - Position {positionNumber} completed");

            }
            return positionRows;
        }

        // Get aggregate scores per stage
        private static List<StageSummary> Aggregate(List<MoveResult> rows, int seedCount)
        {
            // Set up aggregate data
            List<StageSummary> aggregates = new List<StageSummary>();

            // Loop through each stage
            foreach (Stage stage in Enum.GetValues<Stage>())
            {
                Console.WriteLine($"Aggregating {stage} stage results..");

                // Set up counters (set to double for some for calculating final metrics)
                int moves = 0;
                int illegalMoves = 0;
                double legalMoves = 0;
                double agreements = 0;
                double resultPreserved = 0;
                long regretSum = 0;
                double speedRegretSum = 0;
                double speedRegretCount = 0;
                double decisionSum = 0.0;
                double decisionMax = 0.0;
                double nodesSum = 0;
                double nodesCount = 0;
                List<double> decisionTimes = new List<double>();
                // Final metrics
                double agreementRate = 0;
                double meanRegret = 0;
                double resultPreservedRate = 0;
                double meanDecisionMs = 0;
                double? meanSpeedRegret = null;
                double? meanNodes = null;


                // Walk every row, using only the ones for this stage.
                foreach (MoveResult m in rows)
                {
                    // Skip data not in the current stage
                    if (m.Stage != stage)
                    {
                        continue;
                    }

                    // Add necessary information to counters
                    moves++;
                    decisionTimes.Add(m.DecisionMs);
                    decisionSum += m.DecisionMs;
                    
                    // Track latest max time
                    if (m.DecisionMs > decisionMax)
                    {
                        decisionMax = m.DecisionMs;
                    }
                        
                    // If node has value (for minimax), track it
                    if (m.Nodes.HasValue)
                    {
                        nodesSum += m.Nodes.Value;
                        nodesCount++;
                    }
                    // If move is illegal, skip tracking scoring
                    if (m.Illegal)
                    {
                        illegalMoves++;
                        continue; 
                    }

                    // For legal moves, track scores
                    legalMoves++;
                    if (m.Agreement)
                    {
                        agreements++;
                    }
                    if (m.ResultPreserved)
                    {
                        resultPreserved++;
                    }
                    regretSum += m.Regret;
                    if (m.SpeedRegret.HasValue)
                    {
                        speedRegretSum += m.SpeedRegret.Value;
                        speedRegretCount++;
                    }
                }

                // If no rows for this stag, skip it.
                if (moves == 0)
                    continue;

                // Get number of unique positions
                int positions = moves / seedCount;

                // Calculate averages/rates from the counters
                if (legalMoves > 0)
                {
                    agreementRate = agreements / legalMoves;
                    meanRegret = regretSum / legalMoves;
                    resultPreservedRate = resultPreserved / legalMoves;
                }
                if (speedRegretCount > 0)
                {
                    meanSpeedRegret = speedRegretSum / speedRegretCount;
                }
                meanDecisionMs = decisionSum / moves;
                if (nodesCount > 0)
                {
                    meanNodes = nodesSum / nodesCount;
                }
                
                
                // Get 95th-percentile decision time
                decisionTimes.Sort();
                int p95Index = (int)Math.Ceiling(95.0 / 100.0 * decisionTimes.Count) - 1;
                double p95DecisionMs = decisionTimes[p95Index];

                // Add aggregates for that group to data
                aggregates.Add(new StageSummary(
                    stage, positions, moves, illegalMoves,
                    agreementRate, meanRegret, resultPreservedRate, meanSpeedRegret,
                    meanDecisionMs, p95DecisionMs, decisionMax,
                    meanNodes));
            }

            return aggregates;
        }

    }
}
