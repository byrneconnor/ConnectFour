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
                TrainTestSplit.PrintSplitSummary(train, test);

                return;
            }

            // run the benchmark evaluation
            if (args.Contains("--benchmark-evaluation-train"))
            {
                // Read in the train split to evaluate against
                string trainFile = "train-split-" + SplitLabel(populationFraction) + ".json";
                List<SolvedPosition> train = JsonHelpers.Read(Path.Combine(dataFolder, trainFile));

                // Seeds for the repeated MCTS runs (MCTS is stochastic; average across runs).
                // In the benchmark these are per-position move decisions, not full games.
                int numberOfRuns = 5;
                List<int> runSeeds = MakeRunSeeds(numberOfRuns);

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
                    PlayerFactory singlePlayerConfiguration = MakeMinimaxConfiguration(config.Label, config.Depth, config.Weights);

                    // Evaluate this configuration on the train split
                    BenchmarkResult result = BenchmarkEvaluation.Run(
                        config.Label, "train", singlePlayerConfiguration, train, new List<int> { minimaxSeed });

                    // Keep the per-config detail files (uniquely named by the config label)
                    SaveBenchmarkResults.Save(result, dataFolder);

                    BenchmarkEvaluation.PrintEvaluationSummary(result);

                    runs.Add((config, result));
                }

                // Write the combined depth x weight x stage table for ranking + the write-up
                EvaluationTables.SaveMinimaxTuningGrid(runs, dataFolder);


                ///////////////////////////////////////////
                // Evaluate MCTS 
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
                        string explorationLabel = exploration.ToString("0.00");
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
                    PlayerFactory singlePlayerConfiguration =
                        MakeMCTSConfiguration(config.Label, config.Iterations, config.ExplorationConstant);

                    // Evaluate this configuration across every seed (stochastic - average the repetitions)
                    BenchmarkResult result = BenchmarkEvaluation.Run(
                        config.Label, "train", singlePlayerConfiguration, train, runSeeds);

                    // Keep the per-config detail files (uniquely named by the config label)
                    SaveBenchmarkResults.Save(result, dataFolder);

                    BenchmarkEvaluation.PrintEvaluationSummary(result);

                    mctsRuns.Add((config, result));
                }

                // Write the combined iterations x exploration x stage table for ranking + the write-up
                EvaluationTables.SaveMCTSTuningGrid(mctsRuns, dataFolder);

                return;
            }

            // Run test data against chosen configurations to check for overfitting
            if (args.Contains("--benchmark-evaluation-test"))
            {
                // Read in test data
                string label = SplitLabel(populationFraction);
                string testFile = "test-split-" + label + ".json";
                List<SolvedPosition> test = JsonHelpers.Read(Path.Combine(dataFolder, testFile));

                // Seeds for the repeated MCTS runs 
                int numberOfRuns = 5;
                List<int> runSeeds = MakeRunSeeds(numberOfRuns);

                // Combination of minimax and MCTS results
                List<BenchmarkResult> finalResults = new List<BenchmarkResult>();

                ///////////////////////////////////
                // Minimax evaluation
                Console.WriteLine("Evaluating minimax...");

                // Minimax is deterministic given a fixed tie-break seed, so one run suffices.
                int minimaxSeed = 2891;

                // Set the best configuration (TO FINALISE LATER)
                int depth = 8;
                HeuristicWeights weights = new HeuristicWeights();

                // Plug into minimax player
                PlayerFactory minimaxFinal = MakeMinimaxConfiguration("minimax-final", depth, weights);

                // Get results
                BenchmarkResult minimaxResult = BenchmarkEvaluation.Run(
                    "minimax-final", "test", minimaxFinal, test, new List<int> { minimaxSeed });

                // Save minimax run
                SaveBenchmarkResults.Save(minimaxResult, dataFolder);
                
                BenchmarkEvaluation.PrintEvaluationSummary(minimaxResult);
                
                // Add to minimax-MCTS combined results dataset
                finalResults.Add(minimaxResult);

                ///////////////////////////////////////////
                // Evaluate MCTS
                Console.WriteLine("Evaluating MCTS...");

                // Set up chosen results (COMEPLETE LATER)
                List<MctsConfig> chosenMctsConfigs = new List<MctsConfig>
                {
                    new MctsConfig("mcts-final-1", 5000, 0.7),
                    new MctsConfig("mcts-final-2", 5000, 1.41421356237),
                    new MctsConfig("mcts-final-3", 5000, 2.0),
                };

                // Loop through each configuration
                foreach (MctsConfig config in chosenMctsConfigs)
                {
                    Console.WriteLine($"Evaluating {config.Label}...");

                    PlayerFactory mctsFinal =
                        MakeMCTSConfiguration(config.Label, config.Iterations, config.ExplorationConstant);

                    // The harness loops positions x seeds, so each seed is one repeat per position.
                    BenchmarkResult mctsResult = BenchmarkEvaluation.Run(
                        $"{config.Label}-i{config.Iterations}-c{config.ExplorationConstant}", "test", 
                        mctsFinal, test, runSeeds);

                    // Keep the per-config detail files
                    SaveBenchmarkResults.Save(mctsResult, dataFolder);

                    BenchmarkEvaluation.PrintEvaluationSummary(mctsResult);

                    // Add to minimax-MCTS combined results dataset
                    finalResults.Add(mctsResult);
                }

                // Save the full combined set of results
                EvaluationTables.SaveFinalEvaluationTable(finalResults, dataFolder, label);

                return;
            }

            // Run AI v AI arena
            if (args.Contains("--arena-evaluation"))
            {
                // Number of games per pairing (CONFIRM NUMBERS LATER)
                int gamesPerPairing = 10;

                // set seed for reproducibility
                int arenaSeed = 2891;

                // Set up the players (CONFIRM LATER)
                List<IArenaPlayer> players = new List<IArenaPlayer>
                {
                    new RandomPlayer("random", Disc.Red),
                    new MinimaxPlayer("minimax-d8-defensive", Disc.Red, searchDepth: 8, weights: new HeuristicWeights { OpponentTwo = -20, OpponentThree = -120 }),
                    new MCTSPlayer("mcts-5k-c2-00", Disc.Red, totalIterations: 5000, explorationConstant: 2.0),
                };

                // Run the tournament
                Console.WriteLine("Running arena...");
                ArenaResult arenaResult = ArenaEvaluation.Run(players, gamesPerPairing, arenaSeed);

                // Save and print results
                ArenaEvaluation.PrintSummary(arenaResult);
                SaveArenaResults.Save(arenaResult, dataFolder);
                
                return;

            }

            // if no or incorrect arguments passed in the command
            Console.WriteLine("No recognised argument provided. Available flags:");
            Console.WriteLine("  --web-scrape                  Scrape benchmark positions from the solver");
            Console.WriteLine("  --split                       Produce the train/test split");
            Console.WriteLine("  --benchmark-evaluation-train  Tune configurations on the train split");
            Console.WriteLine("  --benchmark-evaluation-test   Evaluate final configurations on the test split");
            Console.WriteLine("  --arena-evaluation            Run the AI vs AI arena evaluation");

        }

        private static List<int> MakeRunSeeds(int numberOfRuns)
        {
            List<int> runSeeds = new List<int>();
            for (int i = 0; i < numberOfRuns; i++)
            {
                runSeeds.Add(i);
            }

            return runSeeds;

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

        // Builds a minimax player for a given configuration
        private static PlayerFactory MakeMinimaxConfiguration(string name, int searchDepth, HeuristicWeights weights)
        {
            Player CreateMinimax(Disc disc, int seed)
            {
                return new MinimaxPlayer(
                    name,
                    disc,
                    searchDepth: searchDepth,
                    weights: weights,
                    seed: seed);
            }

            return CreateMinimax;
        }

        // Builds a MCTS player for a given configuration
        private static PlayerFactory MakeMCTSConfiguration(string name, int totalIterations, double explorationConstant)
        {
            Player CreateMCTS(Disc disc, int seed)
            {
                return new MCTSPlayer(
                    name,
                    disc,
                    totalIterations: totalIterations,
                    explorationConstant: explorationConstant,
                    seed: seed);
            }

            return CreateMCTS;
        }

    }
}