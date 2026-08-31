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

            // Set seed and training fraction
            int seed = 2891;
            double trainFraction = 0.7;

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
                var (train, test) = new TrainTestSplit(data, seed: seed, trainFraction: trainFraction).Split();

                // Save data
                JsonHelpers.Save(Path.Combine(dataFolder, "train-split.json"), train);
                JsonHelpers.Save(Path.Combine(dataFolder, "test-split.json"), test);

                // Print total counts
                Console.WriteLine($"seed={seed}  trainFraction={trainFraction}");
                Console.WriteLine($"Total {data.Count},  train {train.Count}, test {test.Count}");

                // Print group counts
                foreach (Stage stage in Enum.GetValues<Stage>())
                {
                    int trainCount = train.Count(r => SourceFile.GetStage(r.SourceFile) == stage);
                    int testCount = test.Count(r => SourceFile.GetStage(r.SourceFile) == stage);
                    Console.WriteLine($"For {stage} stage, training count is {trainCount}, test count is {testCount}");
                }

                return;
            }

        }
    }
}