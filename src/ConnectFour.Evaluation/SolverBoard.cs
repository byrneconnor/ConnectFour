using ConnectFour.Core;

namespace ConnectFour.Evaluation
{
    public static class SolverBoard
    {
        // For a string of moves, get the appropriate disc colur to 
        // drop into the board
        public static Disc DiscToDrop(int moveTurn)
        {
            if (moveTurn % 2 == 0)
            {
                return Disc.Red;
            }
            else
            {
                return Disc.Yellow;
            }
        }

        public static Board SolverStringPositionToBoard( string position)
        {
            Board board = new();

            for(int i = 0; i < position.Length; i++)
            {
                // Get appropriate disc
                Disc disc = DiscToDrop(i);

                // get the column position
                // solver is 1-indexed columns whereas my code is 0-indexed
                // I will subtract 1 from the value to put it in the correct column
                char c = position[i];
                int column = c - '1';

                // Check column is a legal move
                if (column < 0 || column > 6)
                {
                    throw new FormatException($"Invalid column '{column}' in position '{position}'");
                }

                // Drop disc into board, raise error if false returned
                if (!board.DropDisc(column, disc))
                {
                    throw new InvalidOperationException(
                        $"Illegal move '{column}' from position '{position}'");
                }

            }

            return board;
        }

        public static List<int> FullColumns(Board board)
        {
            // Set up list of full columns
            List<int> full = new();

            // Loop through and add any full columns to list
            for (int c = 0; c < Board.Columns; c++)
            {
                if (!board.IsValidMove(c))
                {
                    full.Add(c);
                }
            }
            return full;
        }
    }
}
