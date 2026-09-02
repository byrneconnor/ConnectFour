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

                static Player MakeMinimax(Disc disc, int runSeed)
                {
                    return new MinimaxPlayer("minimax", disc, searchDepth: 8, seed: runSeed);
                }

                // Evaluate results
                BenchmarkResult minimaxResult = BenchmarkEvaluation.Run(
                    "minimax-d8", 
                    "train", 
                    MakeMinimax, 
                    train, 
                    new List<int> { minimaxSeed }); // Only need one seed for minimax

                // Save and print results
                SaveBenchmarkResults.Save(minimaxResult, dataFolder);
                PrintEvaluationSummary(minimaxResult);


                ///////////////////////////////////////////
                // MCTS - needs several games for averaging
                Console.WriteLine("Evaluating MCTS...");

                // Create the MCTS player
                static Player MakeMcts(Disc disc, int runSeed)
                {
                    return new MCTSPlayer("mcts", disc, totalIterations: 20000, seed: runSeed);
                }

                // Evaluate results
                BenchmarkResult mctsResult = BenchmarkEvaluation.Run(
                    "mcts-20k", 
                    "train",
                    MakeMcts, 
                    train, 
                    runSeeds); // Use multiple seeds as one run of MCTS non-deterministic. Average across seeded repititions for realistic metrics
                
                // Save and print results
                SaveBenchmarkResults.Save(mctsResult, dataFolder);
                PrintEvaluationSummary(mctsResult);

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

    }
}