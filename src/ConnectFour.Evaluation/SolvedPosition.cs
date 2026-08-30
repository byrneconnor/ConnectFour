using ConnectFour.Core;

namespace ConnectFour.Evaluation
{
    // Set up solvedPosition data 
    public sealed record SolvedPosition(
        string Position, // Board position from solver ("6471" means Red to col 6, Yellow to col 4, Red to col 7, Yellow to col 1)
        Disc DiscToMove, // "Red" or "Yellow" (Red moves first)
        int?[] ColumnScores, // online solver uses cols 1-7, our game uses 0-6. null = full column, so can't play that move
        int[] BestColumns, // the column(s) with the best score in ColumnScores
        int Value, // the best score from ColumnScores
        string Outcome, // The best outcome for discToMove, based on Value (negative = loss, 0 = draw, positive = win)
        int ExpectedValue, // the value published in the original data file for cross reference
        bool MatchesExpected, // Value == ExpectedValue (cross reference to check data web scraped accurately)
        string SourceFile) // which file this position came from
    {
        // Method to get the correct disc for current player's turn
        // (Red for odd turns, Yellow for even turns based off of postion)
        public static Disc SideToMove(string position)
        {
            if (position.Length % 2 == 0)
            {
                return Disc.Red;
            }
            else
            {
                return Disc.Yellow;
            }
        }
    }
}
