using ConnectFour.Core;

namespace ConnectFour.AI.Tests
{
    // Method to create a board to test logic.
    // Boards are written as they look on screen: row 0 is the top
    // row, row 5 is the bottom row. 'R' = Red, 'Y' = Yellow, '.' = empty.
    public static class TestBoards
    {
        public static Board CreateBoardForTests(string[] boardRows)
        {
            var board = new Board();

            for (int c = 0; c < Board.Columns; c++)
            {
                for (int r = Board.Rows - 1; r >= 0; r--)
                {
                    char ch = boardRows[r][c];

                    if (ch == 'R')
                    {
                        board.DropDisc(c, Disc.Red);
                    }
                    else if (ch == 'Y')
                    {
                        board.DropDisc(c, Disc.Yellow);
                    }
                    else
                    {
                        break; // Empty cell - the rest of this column is empty
                    }
                }
            }

            return board;
        }
    }
}