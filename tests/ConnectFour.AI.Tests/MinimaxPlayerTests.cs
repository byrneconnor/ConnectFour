using static ConnectFour.AI.Tests.TestBoards;
using ConnectFour.Core;

namespace ConnectFour.AI.Tests
{
    public class MinimaxPlayerTests
    {
        // Create board where AI can win in one move and check it does
        [Fact]
        public void GetMove_TakesImmediateWin()
        {
            Board board = CreateBoardForTests(
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
            Board board = CreateBoardForTests(
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
            Board board = CreateBoardForTests(
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

        // Three possible wins to choose from, picks at least one.
        [Fact]
        public void GetMove_PicksAnyWin()
        {
            Board board = CreateBoardForTests(
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

            int[] winningMoves = [1, 2, 6];

            Assert.Contains(move, winningMoves);
        }

        // Check for legal moves by AI
        [Fact]
        public void GetMove_ReturnsLegalColumn()
        {
            Board board = CreateBoardForTests(
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
            Board board = CreateBoardForTests(
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
            Board board = CreateBoardForTests(
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
            Board board = CreateBoardForTests(
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

        // With the depth limit in place, the AI returns a move from an empty board.
        // Times out after 15 seconds which indicates a failure
        [Fact]
        public void GetMove_DepthLimited_ReturnsFromEmptyBoard()
        {
            var board = new Board();
            var ai = new MinimaxPlayer("AI", Disc.Red);

            // Run on a background task so a regression (no depth limit) fails by timing
            // out rather than hanging the whole test run.
            var search = Task.Run(() => ai.GetMove(board));
            bool finished = search.Wait(TimeSpan.FromSeconds(15));

            Assert.True(finished, "MinimaxPlayer.GetMove did not return in time");
            Assert.True(board.IsValidMove(search.Result));
        }

        // Check legal move made when depth and weights set
        [Fact]
        public void GetMove_CustomWeightsAndDepthReturnsLegalColumn()
        {
            // Set some weights
            var weights = new HeuristicWeights
            {
                CentreDisc = 20,
                OpponentThree = -100
            };
            var board = new Board();
            var ai = new MinimaxPlayer("AI", Disc.Red, searchDepth: 4, weights: weights);

            int move = ai.GetMove(board);

            Assert.True(board.IsValidMove(move));
        }

        // WinScore still prioritised even if heuristic weights are quite large
        // Even with the heuristic weights pushed far above their defaults (but kept below
        // WinScore), a real immediate win must still outrank any heuristic score.
        [Fact]
        public void GetMove_LargeHeuristicWeightsStillTakesImmediateWin()
        {
            Board board = CreateBoardForTests(
                [
                "YR.Y.YR",
                "RYYR.RR",
                "YRRR.YR",
                "RRYY.YY",
                "YRRYYRY",
                "RYRRRYY"
                ]);

            // Large weights set
            var weights = new HeuristicWeights
            {
                AiOne = 50,
                AiTwo = 200,
                AiThree = 1000,
                OpponentOne = -50,
                OpponentTwo = -200,
                OpponentThree = -1000,
                CentreDisc = 100
            };
            var ai = new MinimaxPlayer("AI", Disc.Yellow, weights: weights);

            int move = ai.GetMove(board);

            Assert.Equal(4, move);
        }

    }
}