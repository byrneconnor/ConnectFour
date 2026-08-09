namespace ConnectFour.Core
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
        // boolean value returned to indicate success or failure from attempting to drop disc
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

        // check for four in a row
        public bool CheckWin(Disc disc)
        {
            // Horizontal
            // Loop each row
            for (int r = 0; r < Rows; r++)
            {
                // loop each column (up to max column to get 4 in a row - column 3)
                for (int c = 0; c < Columns - 3; c++)
                {
                    // return true if all 4 cell values match disc value
                    if (grid[r, c] == disc &&
                        grid[r, c + 1] == disc &&
                        grid[r, c + 2] == disc &&
                        grid[r, c + 3] == disc)
                    {
                        return true;
                    }
                }
            }

            // Vertical
            // Loop each column
            for (int c = 0; c < Columns; c++)
            {
                // loop each row (up to max row to get 4-in-a-row - row 2)
                for (int r = 0; r < Rows - 3; r++)
                {
                    // return true if all 4 cell values match disc value
                    if (grid[r, c] == disc &&
                        grid[r + 1, c] == disc &&
                        grid[r + 2, c] == disc &&
                        grid[r + 3, c] == disc)
                    {
                        return true;
                    }
                }
            }

            // Down-right diagonal
            // loop each row (up to max row to get 4-in-a-row - row 2)
            for (int r = 0; r < Rows - 3; r++)
            {
                // loop each column (up to max column to get 4 in a row - column 3)
                for (int c = 0; c < Columns - 3; c++)
                {
                    // return true if all 4 cell values match disc value
                    if (grid[r, c] == disc &&
                        grid[r + 1, c + 1] == disc &&
                        grid[r + 2, c + 2] == disc &&
                        grid[r + 3, c + 3] == disc)
                    {
                        return true;
                    }
                }
            }

            // Up-right diagonal
            // loop each row (starting at row 3 as not possible below that)
            for (int r = 3; r < Rows; r++)
            {
                // loop each column (up to max column to get 4 in a row - column 3)
                for (int c = 0; c < Columns - 3; c++)
                {
                    // return true if all 4 cell values match disc value
                    if (grid[r, c] == disc &&
                        grid[r - 1, c + 1] == disc &&
                        grid[r - 2, c + 2] == disc &&
                        grid[r - 3, c + 3] == disc)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // check if board is full - for a draw/end of game
        public bool IsFull()
        {
            // loop through each column, check if top cell is empty
            // return false if so
            for (int c = 0; c < Columns; c++)
            {
                if (grid[0, c] == Disc.Empty)
                {
                    return false;
                }
            }

            return true;
        }

    }
}