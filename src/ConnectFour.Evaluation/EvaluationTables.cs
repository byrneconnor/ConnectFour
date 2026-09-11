using System.Text;

namespace ConnectFour.Evaluation
{
    // Writes the benchmark tuning grids and the final evaluation table to CSV
    public static class EvaluationTables
    {
        // Writes combined minimax results to csv
        public static void SaveMinimaxTuningGrid(
            List<(MinimaxConfig Config, BenchmarkResult Result)> runs, string outputDir)
        {
            StringBuilder sb = new StringBuilder();

            // Header row
            sb.Append(CsvHelpers.Row(
                "config", "depth", "weightGroup", "stage", "positions",
                "agreementRate", "meanRegret", "meanSpeedRegret",
                "meanDecisionMs", "p95DecisionMs", "maxDecisionMs", "meanNodes")).Append('\n');

            // One row per configuration per stage
            foreach ((MinimaxConfig config, BenchmarkResult result) in runs)
            {
                foreach (StageSummary s in result.StageAggregate)
                {
                    sb.Append(CsvHelpers.Row(
                        config.Label, config.Depth, config.WeightGroup, s.Stage, s.Positions,
                        s.AgreementRate, s.MeanRegret, s.MeanSpeedRegret,
                        s.MeanDecisionMs, s.P95DecisionMs, s.MaxDecisionMs, s.MeanNodes)).Append('\n');
                }
            }

            // Write the combined table
            File.WriteAllText(Path.Combine(outputDir, "minimax-tuning-grid.csv"), sb.ToString());
        }

        // Save MCTS results to csv
        public static void SaveMCTSTuningGrid(
            List<(MctsConfig Config, BenchmarkResult Result)> runs, string outputDir)
        {
            StringBuilder sb = new StringBuilder();

            // Header row
            sb.Append(CsvHelpers.Row(
                "config", "iterations", "explorationConstant", "stage", "positions",
                "agreementRate", "meanRegret", "meanSpeedRegret",
                "meanDecisionMs", "p95DecisionMs", "maxDecisionMs")).Append('\n');

            // One row per configuration per stage
            foreach ((MctsConfig config, BenchmarkResult result) in runs)
            {
                foreach (StageSummary s in result.StageAggregate)
                {
                    sb.Append(CsvHelpers.Row(
                        config.Label, config.Iterations, config.ExplorationConstant, s.Stage, s.Positions,
                        s.AgreementRate, s.MeanRegret, s.MeanSpeedRegret,
                        s.MeanDecisionMs, s.P95DecisionMs, s.MaxDecisionMs)).Append('\n');
                }
            }

            // Write the combined table
            File.WriteAllText(Path.Combine(outputDir, "mcts-tuning-grid.csv"), sb.ToString());
        }

        // Save final table for the evaluation of final configurations on test data
        public static void SaveFinalEvaluationTable(
            List<BenchmarkResult> results, string outputDir, string splitLabel)
        {
            StringBuilder sb = new StringBuilder();

            // Header row
            sb.Append(CsvHelpers.Row(
                "agent", "split", "stage", "positions", "seeds",
                "agreementMean", "agreementStd", "agreementMin", "agreementMax",
                "regretMean", "regretStd", "regretMin", "regretMax",
                "meanDecisionMs", "p95DecisionMs", "maxDecisionMs", "meanNodes")).Append('\n');

            foreach (BenchmarkResult result in results)
            {
                foreach (Stage stage in Enum.GetValues<Stage>())
                {
                    // Per-seed agreement / mean regret over the legal moves in this stage.
                    List<double> perSeedAgreement = new List<double>();
                    List<double> perSeedRegret = new List<double>();

                    foreach (int seed in result.Seeds)
                    {
                        int legal = 0;
                        int agree = 0;
                        long regretSum = 0;

                        foreach (MoveResult m in result.Moves)
                        {
                            // Only this stage + seed; illegal moves are excluded from scoring rates.
                            if (m.Stage != stage || m.Seed != seed || m.Illegal)
                            {
                                continue;
                            }
                            legal++;
                            if (m.Agreement)
                            {
                                agree++;
                            }
                            regretSum += m.Regret;
                        }

                        // No legal moves for this seed/stage, skip.
                        if (legal == 0)
                        {
                            continue;
                        }
                        perSeedAgreement.Add((double)agree / legal);
                        perSeedRegret.Add((double)regretSum / legal);
                    }

                    // No data for this agent/stage, skip the row.
                    if (perSeedAgreement.Count == 0)
                    {
                        continue;
                    }

                    // Positions + timing + nodes come from the harness aggregate for this stage.
                    StageSummary? summary = null;
                    foreach (StageSummary s in result.StageAggregate)
                    {
                        if (s.Stage == stage)
                        {
                            summary = s;
                            break;
                        }
                    }
                    if (summary == null)
                    {
                        continue;
                    }

                    // Distribution across the seeded runs for agreement
                    (double aMean, double aStd, double aMin, double aMax) = Stats.Distribution(perSeedAgreement);

                    // Distribution across seeded runs for regret
                    (double rMean, double rStd, double rMin, double rMax) = Stats.Distribution(perSeedRegret);

                    sb.Append(CsvHelpers.Row(
                        result.PlayerName, result.Split, stage, summary.Positions, result.Seeds.Count,
                        aMean, aStd, aMin, aMax,
                        rMean, rStd, rMin, rMax,
                        summary.MeanDecisionMs, summary.P95DecisionMs, summary.MaxDecisionMs,
                        summary.MeanNodes)).Append('\n');
                }
            }

            File.WriteAllText(
                Path.Combine(outputDir, "final-evaluation-test-" + splitLabel + ".csv"), sb.ToString());
        }
    }
}