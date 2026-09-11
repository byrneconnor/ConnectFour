using System.Diagnostics;

using ConnectFour.AI;
using ConnectFour.Core;

namespace ConnectFour.Evaluation
{
    // Arena evaluation - every player plays the other over a fixed number of games.
    // Players alternate evenly who moves first. A single master random seed generator
    // is used to make the tournament reproducible. 
    public class ArenaEvaluation
    {
        // Run the whole round-robin and return the collected results.
        public static ArenaResult Run(List<ArenaEntry> players, int gamesPerPairing, int seed)
        {
            // Check there is at least 2 players in the arena
            if (players.Count < 2)
            {
                throw new ArgumentException("The arena needs at least two players.");
            }

            // Check for positive integer for number of games per pairing
            if (gamesPerPairing <= 0)
            {
                throw new ArgumentException("gamesPerPairing must be greater than zero.");
            }

            // One master random seed generator determines every per-game seed,
            // so each game is different but the run is reproducible from the tournament seed
            Random masterRandom = new Random(seed);

            // Set up players records
            Dictionary<string, PlayerResults> results = new Dictionary<string, PlayerResults>();
            foreach (ArenaEntry player in players)
            {
                results[player.Name] = new PlayerResults();
            }

            // Collected outputs
            List<GameResult> games = new List<GameResult>();
            List<PairingResult> pairings = new List<PairingResult>();

            // Set up for loop to get each combination of players against each other
            for (int i = 0; i < players.Count; i++)
            {
                for (int j = i + 1; j < players.Count; j++)
                {
                    ArenaEntry playerOne = players[i];
                    ArenaEntry playerTwo = players[j];

                    // Head-to-head results for this pairing (relative to playerOne/playerTwo)
                    int playerOneWins = 0;
                    int playerTwoWins = 0;
                    int draws = 0;

                    // Play the set of games for this pairing
                    for (int g = 0; g < gamesPerPairing; g++)
                    {
                        // Alternate who moves first so results are not biased
                        ArenaEntry first;
                        ArenaEntry second;

                        // Set playerOne to open even games (0, 2, 4 ...),
                        bool playerOneStarts = (g % 2 == 0);
                        if (playerOneStarts)
                        {
                            first = playerOne;
                            second = playerTwo;
                        }
                        else
                        {
                            first = playerTwo;
                            second = playerOne;
                        }
                        

                        // Set up fresh seeds from the master seed 
                        int firstSeed = masterRandom.Next();
                        int secondSeed = masterRandom.Next();

                        // Build fresh agents: the first mover plays Red, the second Yellow
                        Player firstPlayer = first.Create(Disc.Red, firstSeed);
                        Player secondPlayer = second.Create(Disc.Yellow, secondSeed);

                        // Play the game out
                        GamePlay play = PlayGame(firstPlayer, secondPlayer);

                        // Work out who won as a player (null for a draw)
                        ArenaEntry? winningPlayer;
                        if (play.Outcome == GameOutcome.FirstPlayer)
                        {
                            winningPlayer = first;
                        }
                        else if (play.Outcome == GameOutcome.SecondPlayer)
                        {
                            winningPlayer = second;
                        }
                        else
                        {
                            winningPlayer = null;
                        }

                        // Set winning players name
                        string winningName;
                        if (winningPlayer == null)
                        {
                            winningName = "Draw";
                        }
                        else
                        {
                            winningName = winningPlayer.Name;
                        }
                        
                        // Record the game row
                        games.Add(new GameResult(
                            PlayerOne: first.Name,
                            PlayerTwo: second.Name,
                            PlayerOneSeed: firstSeed,
                            PlayerTwoSeed: secondSeed,
                            Winner: winningName,
                            TotalMoves: play.Moves,
                            PlayerOneMs: play.FirstPlayerMs,
                            PlayerTwoMs: play.SecondPlayerMs));

                        // Update the head-to-head results (relative to playerOne/playerTwo)
                        if (winningPlayer == null)
                        {
                            draws++;
                        }
                        else if (winningPlayer == playerOne)
                        {
                            playerOneWins++;
                        }
                        else
                        {
                            playerTwoWins++;
                        }

                        // Update the overall per-player results for both sides
                        RecordGame(results[first.Name], results[second.Name], play);
                    }

                    // Store this pairing's head-to-head result
                    pairings.Add(new PairingResult(
                        PlayerOne: playerOne.Name,
                        PlayerTwo: playerTwo.Name,
                        GamesPlayed: gamesPerPairing,
                        PlayerOneWins: playerOneWins,
                        PlayerTwoWins: playerTwoWins,
                        Draws: draws));
                }
            }

            // Build the final standings from the results
            List<PlayerMetrics> standings = BuildStandings(players, results);

            return new ArenaResult(
                GamesPerPairing: gamesPerPairing,
                Seed: seed,
                Games: games,
                Pairings: pairings,
                Standings: standings);
        }

        // Play a single game
        private static GamePlay PlayGame(Player firstPlayer, Player secondPlayer)
        {
            Game game = new Game(firstPlayer, secondPlayer);

            // Set up tracker for moves
            int moves = 0;
            
            // Set up a stopwatch and trackers for player's move times
            Stopwatch stopwatch = new Stopwatch();
            double firstPlayerMs = 0.0;
            double secondPlayerMs = 0.0;

            // Loop until the game is over
            while (!game.IsOver)
            {
                // get current player
                Player currentPlayer = game.CurrentPlayer;

                // Restart the timer, get the current players move and stop the timer
                stopwatch.Restart();
                int column = currentPlayer.GetMove(game.Board);
                stopwatch.Stop();

                // Add time to appropriate player
                if (currentPlayer == firstPlayer)
                {
                    firstPlayerMs += stopwatch.Elapsed.TotalMilliseconds;
                }
                else
                {
                    secondPlayerMs += stopwatch.Elapsed.TotalMilliseconds;
                }

                // Play respective move
                bool played = game.PlayMove(column);

                // If a player plays an illegal move, raise an error
                if (!played)
                {
                    throw new InvalidOperationException(
                        "Agent returned an illegal move (column " + column + ").");
                }

                // Update moves tracker
                moves++;
            }

            // Map the winning player to the outcome
            GameOutcome outcome;
            if (game.Winner == null)
            {
                outcome = GameOutcome.Draw;
            }
            else if (game.Winner == firstPlayer)
            {
                outcome = GameOutcome.FirstPlayer;
            }
            else
            {
                outcome = GameOutcome.SecondPlayer;
            }

            return new GamePlay(outcome, moves, firstPlayerMs, secondPlayerMs);
        }

        // Obtain each players records from running results
        private static void RecordGame(PlayerResults firstPlayer, PlayerResults secondPlayer, GamePlay play)
        {
            firstPlayer.Games++;
            secondPlayer.Games++;

            // Win / loss / draw from each side's point of view
            if (play.Outcome == GameOutcome.FirstPlayer)
            {
                firstPlayer.Wins++;
                secondPlayer.Losses++;
            }
            else if (play.Outcome == GameOutcome.SecondPlayer)
            {
                secondPlayer.Wins++;
                firstPlayer.Losses++;
            }
            else
            {
                firstPlayer.Draws++;
                secondPlayer.Draws++;
            }

            // Add player move times
            firstPlayer.TotalMs += play.FirstPlayerMs;
            secondPlayer.TotalMs += play.SecondPlayerMs;

            // Get each players total count of moves
            // (add 1 to first player to account for an odd total number of moves)
            int firstPlayerMoves = (play.Moves + 1) / 2;
            int secondPlayerMoves = play.Moves / 2;
            firstPlayer.TotalMoves += firstPlayerMoves;
            secondPlayer.TotalMoves += secondPlayerMoves;
        }

        // Turn the per-player results into standings records
        private static List<PlayerMetrics> BuildStandings(
            List<ArenaEntry> players, Dictionary<string, PlayerResults> results)
        {
            List<PlayerMetrics> standings = new List<PlayerMetrics>();

            // Loop through each player
            foreach (ArenaEntry player in players)
            {
                PlayerResults result = results[player.Name];

                // Get win rate and average decision time
                double winRate = (double)result.Wins / result.Games;
                double meanMs = result.TotalMs / result.TotalMoves;

                // Set up a points system for ranking performance - 1 = win, 0.5 = draw, 0 = loss
                double points = result.Wins + (0.5 * result.Draws);

                // Add results to appropriate player
                standings.Add(new PlayerMetrics(
                    Name: player.Name,
                    GamesPlayed: result.Games,
                    Wins: result.Wins,
                    Losses: result.Losses,
                    Draws: result.Draws,
                    WinRate: winRate,
                    Points: points,
                    MeanDecisionMs: meanMs));
            }

            return standings;
        }

        // The outcome of one played game, how many moves were made, and each side's total think time.
        private record GamePlay(GameOutcome Outcome, int Moves, double FirstPlayerMs, double SecondPlayerMs);

        // Track player results across the arena games
        private class PlayerResults
        {
            public int Games;
            public int Wins;
            public int Losses;
            public int Draws;
            public double TotalMs;
            public int TotalMoves;
        }

        // GameOutcome says which player won a single game (or a draw)
        private enum GameOutcome
        {
            Draw,
            FirstPlayer,
            SecondPlayer
        }

        public static void PrintSummary(ArenaResult result)
        {
            Console.WriteLine($"== Arena: {result.GamesPerPairing} games per pairing (seed {result.Seed}) ==");

            // Head-to-head results
            Console.WriteLine("Head-to-head:");
            foreach (PairingResult p in result.Pairings)
            {
                Console.WriteLine(
                    $"  {p.PlayerOne} vs {p.PlayerTwo}: " +
                    $"{p.PlayerOne} wins: {p.PlayerOneWins}, {p.PlayerTwo} wins: {p.PlayerTwoWins}, " +
                    $"{p.Draws} draws over {p.GamesPlayed} games");
            }

            // Final standings
            Console.WriteLine("Standings:");
            foreach (PlayerMetrics s in result.Standings)
            {
                // use F1 and P1 to format decimals/percentages to 1 decimal place
                Console.WriteLine(
                    $"  {s.Name}: {s.Points:F1} pts | " +
                    $"{s.Wins}W {s.Losses}L {s.Draws}D | win rate {s.WinRate:P1} | " +
                    $"mean {s.MeanDecisionMs:F1} ms/move over {s.GamesPlayed} games");
            }
        }
    }
}