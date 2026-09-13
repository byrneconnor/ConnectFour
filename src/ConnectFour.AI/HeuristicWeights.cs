namespace ConnectFour.AI
{
    // A class containing scores for heuristic evaluation. It has scores for good looking positions
    // for AI (which we should prioritise) and opponents (which we should avoid).
    // We can test these scores as part of the evaluation
    public class HeuristicWeights
    {
        // positive scores for where AI has two or three in a row
        public int AiTwo { get; set; } = 10;
        public int AiThree { get; set; } = 50;

        // negative scores for where human has two or three in a row
        // currently set opponentThree to a higher magnitude to AiThree to prioritise blocking
        public int OpponentTwo { get; set; } = -10;
        public int OpponentThree { get; set; } = -60;

        // Scores per dics on the board to represent disc positions
        public bool UsePositionalWeights { get; set; } = true;
        public int[,] PositionalWeights =
        {
            { 3, 4, 5, 7, 5, 4, 3 },
            { 4, 6, 8, 10, 8, 6, 4 },
            { 5, 8, 11, 13, 11, 8, 5 },
            { 5, 8, 11, 13, 11, 8, 5 },
            { 4, 6, 8, 10, 8, 6, 4 },
            { 3, 4, 5, 7, 5, 4, 3 }
        };

    }
}