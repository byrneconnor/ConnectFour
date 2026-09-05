namespace ConnectFour.Evaluation
{
    // GameResult stores one played game
    public record GameResult(
        string PlayerOne, // Player to move first (Red disc)
        string PlayerTwo, // Player to move second (Yellow disc)
        int PlayerOneSeed, // seed used by player one
        int PlayerTwoSeed, // seed used by player two
        string Winner, // player that won (or draw)
        int TotalMoves, // number of moves in the game
        double PlayerOneMs, // total time player one spent choosing moves
        double PlayerTwoMs); // total time player two spent choosing moves

    // PairingResult stores head-to-head results
    public record PairingResult(
        string PlayerOne, // first player (not indicating player order)
        string PlayerTwo, // second player (not indicating player order)
        int GamesPlayed, // total number of games played (should be the same throughout)
        int PlayerOneWins, // number of wins for player one
        int PlayerTwoWins, // number of wins for player two
        int Draws); // number of draws

    // PlayerMetrics stores results for individual player
    public record PlayerMetrics(
        string Name, // name of player/configuration
        int GamesPlayed, // number of games played
        int Wins, // number of wins
        int Losses, // number of losses
        int Draws, // number of draws
        double WinRate, // win rate
        double Points, // points for ranking players (1 = win, 0.5 = draw, 0 = loss)
        double MeanDecisionMs); // avergae decision time

    // ArenaResults combines the full results
    public record ArenaResult(
        int GamesPerPairing, // set number of games per pairing
        int Seed, // seed used for reproducibility
        List<GameResult> Games,
        List<PairingResult> Pairings,
        List<PlayerMetrics> Standings);
}