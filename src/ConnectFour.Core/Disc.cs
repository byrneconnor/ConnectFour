namespace ConnectFour.Core
{
    // Disc - set values for what a cell could contain
    public enum Disc
    {
        Empty,
        Red,
        Yellow
    }
    public static class DiscExtensions
    {
        // Get opponents disc
        public static Disc Opponent(this Disc disc)
        {
            if (disc == Disc.Red)
            {
                return Disc.Yellow;
            }

            if (disc == Disc.Yellow)
            {
                return Disc.Red;
            }

            throw new ArgumentException("Disc.Empty has no opponent.");
        }
    }
}