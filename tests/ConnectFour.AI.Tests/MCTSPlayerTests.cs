using static ConnectFour.AI.Tests.TestBoards;
using ConnectFour.Core;

namespace ConnectFour.AI.Tests
{
    public class MCTSPlayerTests
    {
        // MCTS is non-deterministic so tests are probabilistic. A high number of iterations 
        // should make MCTS converge to the 'right' move but still not guaranteed
        private const int totalIterations = 20000;

        // Check MCTS returns a legal move
        [Fact]
        public void GetMove_ReturnsLegalMove()
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

            var ai = new MCTSPlayer("AI", Disc.Yellow);

            int move = ai.GetMove(board);

            Assert.True(board.IsValidMove(move));
        }

        // Check MCTS takes the only legal move
        [Fact]
        public void GetMove_ReturnsOnlyOneLegalMoveAvailable()
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

            var ai = new MCTSPlayer("AI", Disc.Yellow);

            int move = ai.GetMove(board);

            Assert.Equal(0, move);
        }

        // Check MCTS returns error for a full board
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

            var ai = new MCTSPlayer("AI", Disc.Red);

            Assert.Throws<InvalidOperationException>(() => ai.GetMove(board));
        }

        // Check MCTS returns a move even with the smallest possible budget (one iteration expands only one child)
        [Fact]
        public void GetMove_MinimalBudgetReturnsLegalMove()
        {
            var board = new Board();
            var ai = new MCTSPlayer("AI", Disc.Red, totalIterations: 1);

            int move = ai.GetMove(board);

            Assert.True(board.IsValidMove(move));
        }

        // Check customised input variables (iterations and exploration constant) return a valid move
        [Fact]
        public void GetMove_CustomIterationsAndExplorationReturnsLegalMove()
        {
            var board = new Board();
            var ai = new MCTSPlayer("AI", Disc.Red, totalIterations: 500, explorationConstant: 0.5);

            int move = ai.GetMove(board);

            Assert.True(board.IsValidMove(move));
        }

        // Non-deterministic test - use default high iteration count
        // Check that MCTS takes winning move
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

            var ai = new MCTSPlayer("AI", Disc.Yellow, totalIterations: totalIterations);

            int move = ai.GetMove(board);

            Assert.Equal(4, move);
        }

        // Non-deterministic test - use default high iteration count
        // Check that MCTS blocks the opponent winning
        [Fact]
        public void GetMove_BlocksImmediateWin()
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

            var ai = new MCTSPlayer("AI", Disc.Red, totalIterations: totalIterations);

            int move = ai.GetMove(board);

            Assert.Equal(4, move);
        }

        // Non-deterministic test - use default high iteration count
        // Check that MCTS takes one of several possible winning moves
        [Fact]
        public void GetMove_PicksAWinWhenSeveralExist()
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

            var ai = new MCTSPlayer("AI", Disc.Red, totalIterations: totalIterations);

            int move = ai.GetMove(board);

            int[] winningMoves = [1, 2, 6];

            Assert.Contains(move, winningMoves);
        }

        // Set up a game - might be superseeded once arena is set up
        // Return the winner of a game between two players
        private static Player? PlayGame(Player playerOne, Player playerTwo)
        {
            // set up game
            var game = new Game(playerOne, playerTwo);
            
            // play until game is over
            while (!game.IsOver)
            {
                // Get current player
                Player current = game.CurrentPlayer;

                // Get players move to make
                int move = current.GetMove(game.Board);

                // play move and swap players turn
                game.PlayMove(move);
            }

            return game.Winner;
        }


        // Non-deterministic test - using lower iterations count but should still
        // win more games than random
        // Check that MCTS comfortably beats random opponent over several games
        // Sides are alternated so the first-move advantage doesn't favour either
        [Fact]
        public void GetMove_BeatsRandomPlayerOverManyGames()
        {
            const int games = 16; // set number of games
            const int budget = 400; // lower budget for faster games

            int wins = 0;
            int losses = 0;
            int draws = 0;

            for (int g = 0; g < games; g++)
            {
                Player mctsPlayer;
                Player randomPlayer;
                Player? winner;

                // Alternate who starts first
                if (g % 2 == 0)
                {
                    // MCTS plays Red (moves first)
                    mctsPlayer = new MCTSPlayer("MCTS", Disc.Red, totalIterations: budget);
                    randomPlayer = new RandomPlayer("Random", Disc.Yellow);
                    winner = PlayGame(mctsPlayer, randomPlayer);
                }
                else
                {
                    // MCTS plays Yellow (moves second); Random (Red) starts
                    randomPlayer = new RandomPlayer("Random", Disc.Red);
                    mctsPlayer = new MCTSPlayer("MCTS", Disc.Yellow, totalIterations: budget);
                    winner = PlayGame(randomPlayer, mctsPlayer);
                }

                if (winner == mctsPlayer)
                {
                    wins++;
                }
                else if (winner == randomPlayer)
                {
                    losses++;
                }
                else
                {
                    draws++;
                }
            }

            Console.WriteLine($"MCTS won {wins}, loss {losses}, drew {draws}");

            Assert.True(wins + losses + draws == games, "Error in test, total number of games not matching win/loss/draw stats");

            Assert.True(wins > losses, $"Expected MCTS to beat Random overall, but MCTS won {wins} and loss {losses}.");
        }
    }
}