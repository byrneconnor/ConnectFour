using ConnectFour.Core;

namespace ConnectFour
{
    public class Board
    {
        // Set up the Connect Four 6 x 7  board
        public const int Rows = 6;
        public const int Columns = 7;

        // Create a 2D array of Discs with dimensions 6 x 7 with empty discs
        private readonly Disc[,] grid = new Disc[Rows, Columns];

        public Board()
        {
            // blank constructor
        }

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

        // drop a disc into the board
        public bool DropDisc(int column, Disc disc)
        {
            // check move is valid
            if (!IsValidMove(column))
            {
                return false;
            }

            // loop through rows to find lowest available cell
            for (int row = Rows - 1; row >= 0; row--)
            {
                if (grid[row, column] == Disc.Empty)
                {
                    grid[row, column] = disc;
                    return true;
                }
            }

            return false;
        }

        // return the value at a given cell
        public Disc CellAt(int row, int column)
        {
            return grid[row, column];
        }

    }
}