namespace ConnectFour.AI
{
    // A class containing scores for heuristic evaluation. It has scores for good looking positions
    // for AI (which we should prioritise) and opponents (which we should avoid).
    // We can test these scores as part of the evaluation
    public class HeuristicWeights
    {
        // positive scores for where AI has one, two or three in a row
        public int AiOne { get; set; } = 1;
        public int AiTwo { get; set; } = 10;
        public int AiThree { get; set; } = 50;

        // negative scores for where human has one, two or three in a row
        // currently set opponentThree to a higher magnitude to AiThree to prioritise blocking
        public int OpponentOne { get; set; } = -1;
        public int OpponentTwo { get; set; } = -10;
        public int OpponentThree { get; set; } = -60;

        // Score per disc held in the centre column (added for the AI, subtracted for the opponent)
        public int CentreDisc { get; set; } = 30;

    }
}