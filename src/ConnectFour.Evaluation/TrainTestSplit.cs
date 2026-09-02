namespace ConnectFour.Evaluation
{
    // Class to produce a train-test split
    // benchmarkJson = file location for the original benchmark data
    // seed = used for reproducibility (get same train-test split)
    // trainFraction = what proportion you want in your training data (default at 70%)
    // populationFraction = what proportion of the full population you want in your train-test split (default at 100%)
    public class TrainTestSplit(List<SolvedPosition> benchmarkJson, int seed, double trainFraction = 0.7, double populationFraction = 1)
    {
        // Split benchmark data into a train-test split
        public (List<SolvedPosition> Train, List<SolvedPosition> Test) Split()
        {
            // Set random seed
            var random = new Random(seed);

            // define the train and test data
            var train = new List<SolvedPosition>();
            var test = new List<SolvedPosition>();

            // Order the data by unique positions so starting order is identical
            var orderedPositions = benchmarkJson.OrderBy(r => r.Position, StringComparer.Ordinal);

            // Process stages in fixed order (beginning, middle, end) so random seed is 
            // used in the same sequence each run
            // Group the ordered records by stage, then order those groups by the Stage enum
            var groups = orderedPositions
                .GroupBy(r => SourceFile.GetStage(r.SourceFile))
                .OrderBy(g => g.Key);

            // Split each stage on its own, so train and test keep the same begin/mid/end proportions
            foreach (var group in groups)
            {
                // Shuffle the records using random seed
                var shuffled = group
                    .OrderBy(_ => random.Next())
                    .ToList();

                // Limit how much of this group is used by populationFraction
                int populationCount = (int)Math.Round(shuffled.Count * populationFraction);

                // Of that limted cut, get the number of records you want for the training data 
                int trainCount = (int)Math.Round(populationCount * trainFraction);

                // Loop through each record within populationCount
                for (int i = 0; i < populationCount; i++)
                {
                    // For records under trainCount put in training
                    if (i < trainCount)
                        train.Add(shuffled[i]);
                    // The rest go in test
                    else
                        test.Add(shuffled[i]);
                }
            }

            return (train, test);
        }

    }
}