using System.Text.Json;

namespace ConnectFour.Evaluation
{
    // We need benchmark data for part of the evaluation. To generate this ourselves would take a long time
    // The URL https://connect4.gamesolver.org/ takes a GET request with a board postion and can return the 
    // score value for each column

    public static class WebscrapeBenchmarkPositions
    {
        // Base URL with the '/solve?pos=' part of the GET request 
        private const string BaseUrl = "https://connect4.gamesolver.org/solve?pos=";

        // Real scores live in [-18, 18]; the solver represents a full/unplayable column as 100, I'll store anything outside +/- 18 as null
        private const int MinScore = -18;
        private const int MaxScore = 18;
        // Set maxRetries incase first time hits an error
        private const int MaxRetries = 5;
        // save data periodically in case of crashes
        private const int SaveEvery = 50;

        // Runs asynchronously so network waits don't block a thread; requests still happen one at a time
        public static async Task RunWebscrapingAsync(
            string dataFolder, // Location of the data folder
            string outputFileName, // Name of output file
            double requestDelaySeconds = 1.5, // delay per request - polite webscraping
            string? contact = null) // contact details - accepts null but code will error if left at null
        {
            // Check the data directory exists
            if (!Directory.Exists(dataFolder))
            {
                throw new DirectoryNotFoundException($"Data folder not found: {dataFolder}");
            }

            // Set up outputPath for storing data
            string outputPath = Path.Combine(dataFolder, outputFileName);

            // Check it ends in .json
            if (Path.GetExtension(outputPath) != ".json")
            {
                throw new Exception($"outputFileName must end in .json: {outputFileName}");
            }

            // Ensure a contact is set - I use a .env file
            if (contact is null)
            {
                throw new Exception("Error: no 'contact' value passed in RunWebscrapingAsync");
            }

            // Set up a User-Agent for polite webscraping with product/version + contact details 
            string userAgent = $"AI-In-Board-Games-Connect-Four/1.0 (+{contact})";

            // Set up HttpClient
            HttpClient httpClient = new HttpClient();
            // Throws TaskCancelledException error if request takes longer the 30 seconds
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            // Attach the User-Agent header
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

            // Read every position file in the data folder
            List<(string position, int expectedScore, string fileName)> data = ReadPositions(dataFolder, outputFileName);

            // Set up list to store webscrape solved positions
            List<SolvedPosition> results = new();

            // Counter to track scraped and errors
            int scraped = 0;
            int errors = 0;
            
            // Loop through data to send GET requests
            for (int i = 0; i < data.Count; i++)
            {
                // Read each records position, score and filename
                (string position, int expectedScore, string fileName) = data[i];

                // Make GET request and return score values as array
                int[]? scores = await FetchScoresAsync(httpClient, position);
                
                // If scores returned null, print message
                if (scores is null)
                {
                    Console.WriteLine($"Data entry {i + 1}/{data.Count} with position {position} returned null scores.");
                    errors++;
                }
                else
                // Otherwise, add scores to record and add record to results
                {
                    SolvedPosition record = BuildRecord(position, scores, expectedScore, fileName);
                    results.Add(record);
                    scraped++;

                    // Save a snapshot of results incase code crashes part way
                    if (scraped % SaveEvery == 0)
                    {
                        JsonHelpers.Save(outputPath, results);
                    }
                }

                // Print out message to console to update progress
                Console.WriteLine($"[{i + 1}/{data.Count}] so far. Total scraped = {scraped}, total errors = {errors}.");

                // Have a delay between scraping - set by requestDelaySeconds
                await Task.Delay(TimeSpan.FromSeconds(requestDelaySeconds));
            }

            // Save the final version of results
            JsonHelpers.Save(outputPath, results);

            // Final print message
            Console.WriteLine($"Code complete. Number of positions scraped: {scraped}. Number of errors: {errors}.");

        }

        // Read board positions and best score from text files
        // Returns a list of postions, best score and file name
        private static List<(string position, int score, string fileName)> ReadPositions(string dataFolder, string outputFileName)
        {
            List<(string, int, string)> data = new();
            
            // Repeat for each file in the data folder location
            foreach (string path in Directory.GetFiles(dataFolder))
            {
                // Get name of file currently getting read
                string fileName = Path.GetFileName(path);
                // only read the txt files
                if (!Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Start counter for line - help with debugging
                int lineNo = 0;

                // Read through each line
                foreach (string rawLine in File.ReadLines(path))
                {
                    // Increase counter
                    lineNo++;

                    // Trim whitespace from line
                    string line = rawLine.Trim();

                    // If there is nothing left, skip this line
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    // Get the two parts of the text: position and score
                    string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                    
                    // If we don't have 2 exact parts, raise error
                    if (parts.Length != 2)
                    {
                        throw new FormatException($"{fileName} line {lineNo}: expected '<position> <score>'.");
                    }

                    // Assign position to first part
                    string position = parts[0];

                    // Check postion can convert to an integer
                    if (!int.TryParse(parts[1], out int expectedScore))
                    {
                        throw new FormatException($"{fileName} line {lineNo}: score '{parts[1]}' is not an integer.");
                    }

                    // Check each position has a valid column number
                    foreach (char ch in position)
                    {
                        // Should be between 1 and 7 (which I'll convert to 0 to 6 later)
                        if (ch < '1' || ch > '7')
                        {
                            throw new FormatException($"{fileName} line {lineNo}: invalid move character '{ch}'.");
                        }
                    }

                    // Add this iteration to data
                    data.Add((position, expectedScore, fileName));
                    
                }
            }
            
            return data;
        }

        // Method to send GET request and return score values as array
        // If it fails at first tries again (number of retries set by MaxRetries)
        // If null is returned, MaxRetries failed.
        private static async Task<int[]?> FetchScoresAsync(HttpClient httpClient, string position)
        {
            // Concatenate base url with position digits
            string url = BaseUrl + Uri.EscapeDataString(position);

            // Loop for maxRetries
            for (int i = 1; i <= MaxRetries; i++)
            {
                try
                {
                    // Send get request and capture respone
                    using HttpResponseMessage response = await httpClient.GetAsync(url);

                    // if successful, parse response data
                    if (response.IsSuccessStatusCode)
                    {
                        string body = await response.Content.ReadAsStringAsync();
                        return ParseScores(body, position);
                    }

                    // if rate limit hit (429) or possibly temporarily unavailable (503), wait and retry.
                    if ((int)response.StatusCode is 429 or 503)
                    {
                        // Set wait before retrying
                        TimeSpan wait = TimeSpan.FromSeconds(30);
                        // Print delay message to console
                        Console.WriteLine($"Status code {(int)response.StatusCode} for {position} - waiting {wait.TotalSeconds:0.#}s (attempt {i}/{MaxRetries}).");
                        await Task.Delay(wait);
                        continue;
                    }

                    // If any other status, return null
                    Console.WriteLine($"Unexpected status code - {(int)response.StatusCode} for {position}.");
                    return null;
                }
                // catch special cases (connection failure and timeout) and try again
                catch (Exception e)
                    when ((e is HttpRequestException || e is TaskCanceledException))
                {
                    TimeSpan wait = TimeSpan.FromSeconds(30);
                    Console.WriteLine($"Error for {position} ({e.GetType().Name}); retrying in {wait.TotalSeconds:0.#}s (attempt {i}/{MaxRetries}).");
                    await Task.Delay(wait);
                }
            }
            return null;
        }

        // Method to parse score values for each column from the GET response
        private static int[] ParseScores(string body, string position)
        {
            try
            {
                // Parse the JSON
                using JsonDocument doc = JsonDocument.Parse(body);
                // Get property elemnt
                JsonElement score = doc.RootElement.GetProperty("score");
                // Convert to array of inetgers
                int[] result = new int[score.GetArrayLength()];
                int i = 0;
                foreach (JsonElement element in score.EnumerateArray())
                {
                    result[i++] = element.GetInt32();
                }
                return result;
            }
            
            // Catch JSON erros (not valid Json, not right shape) and if 'score' not found
            catch (Exception e) when (e is JsonException or KeyNotFoundException)
            {
                string preview = body.Length > 200 ? body[..200] : body;
                throw new InvalidOperationException(
                    $"Response for '{position}' wasn't the expected JSON (check BaseUrl). First 200 chars:\n{preview}");
            }
        }

        // Gather all the data together to put in record
        private static SolvedPosition BuildRecord(string position, int[] rawScores, int expectedScore, string fileName)
        {
            // Get array of scores
            int?[] columns = new int?[rawScores.Length];

            // Check each score is a credible score (between set MinScore and MaxScore)
            for (int c = 0; c < rawScores.Length; c++)
            {
                int score = rawScores[c];

                if (MinScore <= score && score <= MaxScore)
                {
                    columns[c] = score;
                }
                // if not, set value to null
                else
                {
                    columns[c] = null;
                }
            }

            // Get the maximum score
            int? max = columns.Max();

            // If no playable columns, throw error
            if (max is null)
            {
                throw new InvalidOperationException($"Position {position}: solver returned no playable columns.");
            }

            // We know max is not null, so get its int value
            int best = max.Value;

            // Store columns with best score (may be more than one)
            List<int> bestColumns = new();
            for (int c = 0; c < columns.Length; c++)
            {
                if (columns[c] == best)
                {
                    bestColumns.Add(c);
                }
            }

            // Return the winner
            string outcome; 
            if (best > 0)
            {
                outcome = "Win";
            } 
            else if (best < 0)
            {
                outcome = "Loss";
            }
            else
            {
                outcome = "Draw";
            }
            
            // Return all variables to SolvedPosition
            return new SolvedPosition(
                Position: position,
                DiscToMove: SolvedPosition.SideToMove(position),
                ColumnScores: columns,
                BestColumns: bestColumns.ToArray(),
                Value: best,
                Outcome: outcome,
                ExpectedValue: expectedScore,
                MatchesExpected: best == expectedScore,
                SourceFile: fileName);
        }
    }
}