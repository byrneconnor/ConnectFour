using ConnectFour.Core;

namespace ConnectFour
{
    public class Board
    {
        // Set up the Connect Four 6 x 7  board
        public const int Rows = 6;
        public const int Columns = 7;

        // Create a 2D array of Discs with dimensions 6 x 7
        private readonly Disc[,] grid = new Disc[Rows, Columns];

        // Check if a move is valid
        public bool IsValidMove(int column)
        {
            // is it a valid columns?
            if (column < 0 || column >= Columns)
            {
                return false;
            }

            // return if the top row is empty - true means that column is valid, false otherwise
            return grid[0, column] == Disc.Empty;
        }
    }
}