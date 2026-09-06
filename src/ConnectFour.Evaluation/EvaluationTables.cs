using System.Text;

namespace ConnectFour.Evaluation
{
    // Writes the benchmark tuning grids and the final evaluation table to CSV.
    // Moved out of Program to keep Main focused on wiring the runs together.
    public static class EvaluationTables
    {
        // Writes combined minimax results to csv
        public static void SaveMinimaxTuningGrid(
            List<(MinimaxConfig Config, BenchmarkResult Result)> runs, string outputDir)
        {
            // Set up the csv headers
            StringBuilder sb = new StringBuilder();
            sb.Append("config,depth,weightGroup,stage,positions,agreementRate,meanRegret,meanSpeedRegret,meanDecisionMs,p95DecisionMs,maxDecisionMs,meanNodes\n");

            // One row per configuration per stage
            foreach ((MinimaxConfig config, BenchmarkResult result) in runs)
            {
                foreach (StageSummary s in result.StageAggregate)
                {
                    sb.Append(config.Label).Append(',');
                    sb.Append(config.Depth).Append(',');
                    sb.Append(config.WeightGroup).Append(',');
                    sb.Append(s.Stage).Append(',');
                    sb.Append(s.Positions).Append(',');
                    sb.Append(s.AgreementRate.ToString()).Append(',');
                    sb.Append(s.MeanRegret.ToString()).Append(',');
                    sb.Append(s.MeanSpeedRegret?.ToString()).Append(',');
                    sb.Append(s.MeanDecisionMs.ToString()).Append(',');
                    sb.Append(s.P95DecisionMs.ToString()).Append(',');
                    sb.Append(s.MaxDecisionMs.ToString()).Append(',');
                    sb.Append(s.MeanNodes?.ToString()).Append('\n');
                }
            }

            // Write the combined table
            File.WriteAllText(Path.Combine(outputDir, "minimax-tuning-grid.csv"), sb.ToString());
        }

        // Save MCTS results to csv
        public static void SaveMCTSTuningGrid(
            List<(MctsConfig Config, BenchmarkResult Result)> runs, string outputDir)
        {
            // Set up the csv headers
            StringBuilder sb = new StringBuilder();
            sb.Append("config,iterations,explorationConstant,stage,positions,agreementRate,meanRegret,meanSpeedRegret,meanDecisionMs,p95DecisionMs,maxDecisionMs\n");

            // One row per configuration per stage
            foreach ((MctsConfig config, BenchmarkResult result) in runs)
            {
                foreach (StageSummary s in result.StageAggregate)
                {
                    sb.Append(config.Label).Append(',');
                    sb.Append(config.Iterations).Append(',');
                    sb.Append(config.ExplorationConstant.ToString()).Append(',');
                    sb.Append(s.Stage).Append(',');
                    sb.Append(s.Positions).Append(',');
                    sb.Append(s.AgreementRate.ToString()).Append(',');
                    sb.Append(s.MeanRegret.ToString()).Append(',');
                    sb.Append(s.MeanSpeedRegret.ToString()).Append(',');
                    sb.Append(s.MeanDecisionMs.ToString()).Append(',');
                    sb.Append(s.P95DecisionMs.ToString()).Append(',');
                    sb.Append(s.MaxDecisionMs.ToString()).Append('\n');
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
            sb.Append("agent,split,stage,positions,seeds,");
            sb.Append("agreementMean,agreementStd,agreementMin,agreementMax,");
            sb.Append("regretMean,regretStd,regretMin,regretMax,");
            sb.Append("meanDecisionMs,p95DecisionMs,maxDecisionMs,meanNodes\n");

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

                        // No legal moves for this seed/stage (e.g. all illegal - a bug flag): skip.
                        if (legal == 0)
                        {
                            continue;
                        }
                        perSeedAgreement.Add((double)agree / legal);
                        perSeedRegret.Add((double)regretSum / legal);
                    }

                    // No data for this agent/stage: skip the row.
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

                    sb.Append(result.PlayerName).Append(',');
                    sb.Append(result.Split).Append(',');
                    sb.Append(stage).Append(',');
                    sb.Append(summary.Positions).Append(',');
                    sb.Append(result.Seeds.Count).Append(',');
                    sb.Append(aMean.ToString()).Append(',');
                    sb.Append(aStd.ToString()).Append(',');
                    sb.Append(aMin.ToString()).Append(',');
                    sb.Append(aMax.ToString()).Append(',');
                    sb.Append(rMean.ToString()).Append(',');
                    sb.Append(rStd.ToString()).Append(',');
                    sb.Append(rMin.ToString()).Append(',');
                    sb.Append(rMax.ToString()).Append(',');
                    sb.Append(summary.MeanDecisionMs.ToString()).Append(',');
                    sb.Append(summary.P95DecisionMs.ToString()).Append(',');
                    sb.Append(summary.MaxDecisionMs.ToString()).Append(',');
                    sb.Append(summary.MeanNodes?.ToString()).Append('\n');
                }
            }

            File.WriteAllText(
                Path.Combine(outputDir, "final-evaluation-test-" + splitLabel + ".csv"), sb.ToString());
        }
    }
}