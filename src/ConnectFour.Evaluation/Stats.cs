namespace ConnectFour.Evaluation
{
    // Stores some stat methods used for evaluation (avoids nulls in list)
    public static class Stats
    {
        // Calculate the means 
        public static double Mean(List<double> values)
        {
            double sum = 0.0;
            foreach (double v in values)
            {
                sum += v;
            }
            return sum / values.Count;
        }

        // Calculate standard deviation 
        public static double SampleStdDev(List<double> values)
        {
            if (values.Count < 2)
            {
                return 0.0;
            }

            double mean = Mean(values);
            double sqSum = 0.0;
            foreach (double v in values)
            {
                double d = v - mean;
                sqSum += d * d;
            }
            return Math.Sqrt(sqSum / (values.Count - 1));
        }

        // CAlculate chosen percentile
        public static double Percentile(IReadOnlyList<double> values, double percentile)
        {
            List<double> sorted = new List<double>(values);
            sorted.Sort();

            int index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
            if (index < 0)
            {
                index = 0;
            }
            if (index >= sorted.Count)
            {
                index = sorted.Count - 1;
            }
            return sorted[index];
        }

        // Get a distribution of mean, std, min and max 
        public static (double Mean, double Std, double Min, double Max) Distribution(List<double> values)
        {
            double min = double.PositiveInfinity;
            double max = double.NegativeInfinity;
            foreach (double v in values)
            {
                if (v < min)
                {
                    min = v;
                }
                if (v > max)
                {
                    max = v;
                }
            }

            return (Mean(values), SampleStdDev(values), min, max);
        }
    }
}