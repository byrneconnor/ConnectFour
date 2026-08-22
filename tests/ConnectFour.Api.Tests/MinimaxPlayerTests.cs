using ConnectFour.AI;
using ConnectFour.Core;

namespace ConnectFour.Api.Tests
{
    public class MinimaxPlayerTests
    {
        // Method to create a board to test logic.
        //
        // Boards are written as they look on screen: boardRows[0] is the TOP
        // row, boardRows[5] is the BOTTOM row. 'R' = Red, 'Y' = Yellow, '.' = empty.
        //
        private static Board CreateBoard(string[] boardRows)
        {
            var board = new Board();

            // Loop through each column
            for (int c = 0; c < Board.Columns; c++)
            {
                // Work bottom-to-top so gravity matches the drop order
                for (int r = Board.Rows - 1; r >= 0; r--)
                {
                    // Get the symbol representing a specific cell from the string
                    char ch = boardRows[r][c];

                    // Drop the appropriate disc based on symbol
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

        // Create board where AI can win in one move and check it does
        [Fact]
        public void GetMove_TakesImmediateWin()
        {
            Board board = CreateBoard(
                [
                "YR.Y.YR",
                "RYYR.RR",
                "YRRR.YR",
                "RRYY.YY",
                "YRRYYRY",
                "RYRRRYY"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Yellow);

            int move = ai.GetMove(board);

            Assert.Equal(4, move);
        }

        // Create board to check AI blocks winning move
        [Fact]
        public void GetMove_BlockImmediateWin()
        {
            Board board = CreateBoard(
                [
                "YR.Y.YR",
                "RYYR.RR",
                "YRRR.YR",
                "RRYY.YY",
                "YRRYYRY",
                "RYRRRYY"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Red);

            int move = ai.GetMove(board);

            Assert.Equal(4, move);
        }

        // Create board to check AI picks win over blocking
        [Fact]
        public void GetMove_PicksWinOverBlock()
        {
            Board board = CreateBoard(
                [
                "..R.RR.",
                "Y.RYRYY",
                "Y.YYRRY",
                "YRRRYYY",
                "RYRYRRR",
                "RYRRYYY"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Red);

            int move = ai.GetMove(board);

            Assert.Equal(3, move);
        }

        // Multiple possible wins to choose from, picks at least one.
        [Fact]
        public void GetMove_PicksEitherWin()
        {
            Board board = CreateBoard(
                [
                "...RRR.",
                "Y.RYRYY",
                "Y.YYRRY",
                "RYRRYYY",
                "RRYYRRR",
                "RYRRYYY"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Red);

            int move = ai.GetMove(board);
            
            // 3 possible moves to choose from
            int[] winningMoves = [1, 2, 6];
            
            Assert.Contains(move, winningMoves);
        }

        // Check for legal moves by AI
        [Fact]
        public void GetMove_ReturnsLegalColumn()
        {
            Board board = CreateBoard(
                [
                "YR.Y.YR",
                "RYYR.RR",
                "YRRR.YR",
                "RRYY.YY",
                "YRRYYRY",
                "RYRRRYY"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Yellow);

            int move = ai.GetMove(board);

            Assert.True(board.IsValidMove(move));
        }

        // Check that AI picks the only move to avoid a loss
        [Fact]
        public void GetMove_PlaysOnlyNonLosingMove()
        {
            Board board = CreateBoard(
                [
                "YYR..R.",
                "RYY..RY",
                "YYR.YYY",
                "RRY.RRR",
                "RYYRYYR",
                "RRRYYRR"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Yellow);

            int move = ai.GetMove(board);

            Assert.Equal(3, move); 
        }

        // Check AI picks a draw over a loss
        [Fact]
        public void GetMove_PickDrawOverLoss()
        {
            Board board = CreateBoard(
                [
                ".YY.YYR",
                "YRRYRRR",
                "RYRYRYR",
                "YRRYRRY",
                "YRYRYRY",
                "YYYRRYR"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Red);

            int move = ai.GetMove(board);

            Assert.Equal(3, move); 
        }

        // Check AI picks only avaiable move when board is close to full
        [Fact]
        public void GetMove_FillsLastCellForDraw()
        {
            Board board = CreateBoard(
                [
                ".YYRYYR",
                "YRRYRRR",
                "RYRYRYR",
                "YRRYRRY",
                "YRYRYRY",
                "YYYRRYR"
                ]);

            var ai = new MinimaxPlayer("AI", Disc.Yellow);

            int move = ai.GetMove(board);

            Assert.Equal(0, move);
        }

        // Check full board throws error
        [Fact]
        public void GetMove_FullBoardThrowsError()
        {
            // Create full board of reds
            var board = new Board();
            for (int c = 0; c < Board.Columns; c++)
            {
                for (int r = 0; r < Board.Rows; r++)
                {
                    board.DropDisc(c, Disc.Red);
                }
            }

            var ai = new MinimaxPlayer("AI", Disc.Red);

            Assert.Throws<InvalidOperationException>(() => ai.GetMove(board));
        }
    }
}