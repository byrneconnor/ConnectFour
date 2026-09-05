using System.Globalization;
using System.Text;

using ConnectFour.AI;
using ConnectFour.Core;

using DotNetEnv;

namespace ConnectFour.Evaluation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Load .env file from working directory (should be repo root)
            Env.Load();

            // Get contact from .env file
            string? contact = Environment.GetEnvironmentVariable("WEBSCRAPING_CONTACT");

            // Data folder
            string dataFolder = "src/ConnectFour.Evaluation/data";

            // File output name
            string outputFileName = "benchmark-positions-scraped.json";

            // Set population fraction - how much of the benchmark data do you want to use for the train-test split
            double populationFraction = 0.1;

            // Set seed for train-test split
            int splitSeed = 2891;

            // 6 downloaded data files from http://blog.gamesolver.org/solving-connect-four/02-test-protocol/
            if (args.Contains("--web-scrape"))
            {
                await WebscrapeBenchmarkPositions.RunWebscrapingAsync(
                dataFolder: dataFolder, 
                contact: contact,
                outputFileName: outputFileName);
            }

            // get the train-test split
            if (args.Contains("--split"))
            {
                // Get filepath name
                string benchmarkDataPath = Path.Combine(dataFolder, outputFileName);

                // Read the data in
                var data = JsonHelpers.Read(benchmarkDataPath);

                // Split data using a fixed seed
                var (train, test) = new TrainTestSplit(
                    data,
                    seed: splitSeed,
                    populationFraction: populationFraction
                ).Split();

                // Define a label and set it based on whether we use the full data or not based on population fraction
                string label = SplitLabel(populationFraction);
                
                // Save data
                JsonHelpers.Save(Path.Combine(dataFolder, "train-split-" + label + ".json"), train);
                JsonHelpers.Save(Path.Combine(dataFolder, "test-split-" + label + ".json"), test);

                // print summary
                PrintSplitSummary(train, test);

                return;
            }

            // run the benchmark evaluation
            if (args.Contains("--benchmark-evaluation"))
            {
                // Read in the train split to evaluate against
                string trainFile = "train-split-" + SplitLabel(populationFraction) + ".json";
                List<SolvedPosition> train = JsonHelpers.Read(Path.Combine(dataFolder, trainFile));

                // Seeds for the repeated MCTS runs (MCTS is stochastic; average across runs).
                // In the benchmark these are per-position move decisions, not full games.
                int numberOfRuns = 5;
                List<int> runSeeds = new List<int>();
                for (int i = 0; i < numberOfRuns; i++)
                {
                    runSeeds.Add(i);
                }

                ///////////////////////////////////
                // Minimax evaluation
                Console.WriteLine("Evaluating minimax...");

                // Minimax seed
                int minimaxSeed = 2891;

                // Search depths for configurations: 1 puts emphasis on the weights with no lookahead,
                // 4 and 8 are even lookaheads (CONFIRM FINAL WEIGHTS LATER)
                int[] depths = { 1, 4, 8 };

                // Weight for configuration. Each differs in a meaningful ratio 
                // (CONFIRM WEIGHTS LATER)
                (string Name, HeuristicWeights Weights)[] weightGroups =
                {
                    // Defaults
                    ("a-baseline",  new HeuristicWeights()),   
                    // Prioritise centre column
                    ("b-centre",    new HeuristicWeights { CentreDisc = 60 }),
                    // Prioritise blocking
                    ("c-defensive", new HeuristicWeights { OpponentTwo = -20, OpponentThree = -120 }),
                };

                // Build every depth x weight-group combination
                List<MinimaxConfig> minimaxConfigs = new List<MinimaxConfig>();
                foreach (int depth in depths)
                {
                    foreach ((string groupName, HeuristicWeights weights) in weightGroups)
                    {
                        minimaxConfigs.Add(new MinimaxConfig($"minimax-d{depth}-{groupName}", depth, groupName, weights));
                    }
                }

                // Run each configuration through the harness, keeping the config alongside
                // its result so the summary table can label each row
                List<(MinimaxConfig Config, BenchmarkResult Result)> runs =
                    new List<(MinimaxConfig, BenchmarkResult)>();
                foreach (MinimaxConfig config in minimaxConfigs)
                {
                    Console.WriteLine($"Tuning {config.Label}...");

                    // Return player with single configuration
                    CreatePlayer singlePlayerConfiguration = MakeMinimaxConfiguration(config.Depth, config.Weights);

                    // Evaluate this configuration on the train split
                    BenchmarkResult result = BenchmarkEvaluation.Run(
                        config.Label, "train", singlePlayerConfiguration, train, new List<int> { minimaxSeed });

                    // Keep the per-config detail files (uniquely named by the config label)
                    SaveBenchmarkResults.Save(result, dataFolder);

                    PrintEvaluationSummary(result);

                    runs.Add((config, result));
                }

                // Write the combined depth x weight x stage table for ranking + the write-up
                SaveMinimaxTuningGrid(runs, dataFolder);


                ///////////////////////////////////////////
                // MCTS - needs several games for averaging
                Console.WriteLine("Evaluating MCTS...");

                // Set iteration options
                int[] iterationBudgets = { 5000, 20000, 40000 };

                // Set exploration constant options
                double[] explorationConstants = { 0.7, 1.41421356237, 2.0 };

                // Set up each configuration for MCTS
                List<MctsConfig> mctsConfigs = new List<MctsConfig>();
                foreach (int iterations in iterationBudgets)
                {
                    foreach (double exploration in explorationConstants)
                    {
                        string explorationLabel = exploration.ToString("0.00", CultureInfo.InvariantCulture);
                        mctsConfigs.Add(new MctsConfig($"mcts-i{iterations}-c{explorationLabel}", iterations, exploration));
                    }
                }

                // Set up the results
                List<(MctsConfig Config, BenchmarkResult Result)> mctsRuns =
                    new List<(MctsConfig, BenchmarkResult)>();
                
                // Loop through each config
                foreach (MctsConfig config in mctsConfigs)
                {
                    Console.WriteLine($"Tuning {config.Label}...");

                    // Return player with single configuration
                    CreatePlayer singlePlayerConfiguration =
                        MakeMCTSConfiguration(config.Iterations, config.ExplorationConstant);

                    // Evaluate this configuration across every seed (stochastic - average the repetitions)
                    BenchmarkResult result = BenchmarkEvaluation.Run(
                        config.Label, "train", singlePlayerConfiguration, train, runSeeds);

                    // Keep the per-config detail files (uniquely named by the config label)
                    SaveBenchmarkResults.Save(result, dataFolder);

                    PrintEvaluationSummary(result);

                    mctsRuns.Add((config, result));
                }

                // Write the combined iterations x exploration x stage table for ranking + the write-up
                SaveMCTSTuningGrid(mctsRuns, dataFolder);

                return;
            }

        }

        private static void PrintSplitSummary(List<SolvedPosition> train, List<SolvedPosition> test)
        {
            foreach (Stage stage in Enum.GetValues<Stage>())
            {
                int trainCount = 0;
                int testCount = 0;

                foreach (SolvedPosition position in train)
                {
                    if (SourceFile.GetStage(position.SourceFile) == stage)
                    {
                        trainCount++;
                    }
                }

                foreach (SolvedPosition position in test)
                {
                    if (SourceFile.GetStage(position.SourceFile) == stage)
                    {
                        testCount++;
                    }
                }

                Console.WriteLine(
                    $"For {stage} stage, training count is {trainCount}, test count is {testCount}");
            }
        }

        private static void PrintEvaluationSummary(BenchmarkResult result)
        {
            Console.WriteLine($"== {result.PlayerName} ({result.Split}) ==");
            foreach (StageSummary s in result.StageAggregate)
            {
                Console.WriteLine(
                    $"  {s.Stage}: agreement {s.AgreementRate:P1}, mean regret {s.MeanRegret:F2}, " +
                    $"mean {s.MeanDecisionMs:F1} ms over {s.Positions} positions");
            }
        }

        private static string SplitLabel(double populationFraction)
        {
            if (populationFraction < 1.0)
            {
                return "small";
            }
            else
            {
                return "full";
            }
        }

        // One minimax configuration in the tuning sweep with a label, search depth,
        // the weight-group name, and the weights themselves
        private sealed record MinimaxConfig(string Label, int Depth, string WeightGroup, HeuristicWeights Weights);

        // One MCTS config with a label, iterations limit and exporation constant
        private sealed record MctsConfig(string Label, int Iterations, double ExplorationConstant);

        // Builds a minimax player for a given configuration
        private static CreatePlayer MakeMinimaxConfiguration(int searchDepth, HeuristicWeights weights)
        {
            Player CreateMinimax(Disc disc, int seed)
            {
                return new MinimaxPlayer(
                    "minimax",
                    disc,
                    searchDepth: searchDepth,
                    weights: weights,
                    seed: seed);
            }

            return CreateMinimax;
        }

        // Builds a MCTS player for a given configuration
        private static CreatePlayer MakeMCTSConfiguration(int totalIterations, double explorationConstant)
        {
            Player CreateMCTS(Disc disc, int seed)
            {
                return new MCTSPlayer(
                    "mcts",
                    disc,
                    totalIterations: totalIterations,
                    explorationConstant: explorationConstant,
                    seed: seed);
            }

            return CreateMCTS;
        }

        // Writes combined minimax results to csv
        private static void SaveMinimaxTuningGrid(
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
                    sb.Append(s.AgreementRate.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.MeanRegret.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.MeanSpeedRegret?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',');
                    sb.Append(s.MeanDecisionMs.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.P95DecisionMs.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.MaxDecisionMs.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.MeanNodes?.ToString(CultureInfo.InvariantCulture) ?? "").Append('\n');
                }
            }

            // Write the combined table
            File.WriteAllText(Path.Combine(outputDir, "minimax-tuning-grid.csv"), sb.ToString());
        }

        // Save MCTS results to csv
        private static void SaveMCTSTuningGrid(
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
                    sb.Append(config.ExplorationConstant.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.Stage).Append(',');
                    sb.Append(s.Positions).Append(',');
                    sb.Append(s.AgreementRate.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.MeanRegret.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.MeanSpeedRegret?.ToString(CultureInfo.InvariantCulture) ?? "").Append(',');
                    sb.Append(s.MeanDecisionMs.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.P95DecisionMs.ToString(CultureInfo.InvariantCulture)).Append(',');
                    sb.Append(s.MaxDecisionMs.ToString(CultureInfo.InvariantCulture)).Append('\n');
                }
            }

            // Write the combined table
            File.WriteAllText(Path.Combine(outputDir, "mcts-tuning-grid.csv"), sb.ToString());
        }

    }
}