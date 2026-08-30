using DotNetEnv;

namespace ConnectFour.Evaluation
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Load .env file from working directory (should be repo root)
            Env.Load();

            string? contact = Environment.GetEnvironmentVariable("WEBSCRAPING_CONTACT");

            await WebscrapeBenchmarkPositions.RunWebscrapingAsync(
                dataFolder: "src/ConnectFour.Evaluation/data", // 6 downloaded data files from http://blog.gamesolver.org/solving-connect-four/02-test-protocol/
                contact: contact,
                outputFileName: "benchmark-positions-scraped.json");
        }
    }
}