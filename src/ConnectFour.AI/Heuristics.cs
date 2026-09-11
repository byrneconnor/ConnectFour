using ConnectFour.Core;

namespace ConnectFour.AI
{
    public class Heuristics
    {
        // Heuristic evaluation for non-terminal nodes - scan the board to score cells that
        // look more promising
        public static int HeuristicEvaluation(BoardCopy board, Disc aiDisc, Disc opponentDisc, HeuristicWeights weights)
        {
            // Set a score
            int score = 0;

            // Reward the AI (and penalise the opponent) for holding cells in centre column
            int centre = Board.Columns / 2;
            for (int r = 0; r < Board.Rows; r++)
            {
                Disc cell = board.CellAt(r, centre);
                if (cell == aiDisc)
                {
                    score += weights.CentreDisc;
                } 
                else if (cell == opponentDisc)
                {
                    score -= weights.CentreDisc;
                }
            }

            // Check all horizontal rows and update score
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c <= Board.Columns - 4; c++)
                {
                    score += HeuristicScanCells(board, r, c, 0, 1, aiDisc, opponentDisc, weights);
                }
            }

            // Check for all vertical windows and update score
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                for (int c = 0; c < Board.Columns; c++)
                {
                    score += HeuristicScanCells(board, r, c, 1, 0, aiDisc, opponentDisc, weights);
                }
            }

            // Check for all top left to bottom right (\) and update score
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                for (int c = 0; c <= Board.Columns - 4; c++)
                {
                    score += HeuristicScanCells(board, r, c, 1, 1, aiDisc, opponentDisc, weights);
                }
            }

            // Check for all bottom left to top right (/) and update score
            for (int r = 3; r < Board.Rows; r++)
            {
                for (int c = 0; c <= Board.Columns - 4; c++)
                {
                    score += HeuristicScanCells(board, r, c, -1, 1, aiDisc, opponentDisc, weights);
                }
            }

            return score;
        }

        // Track discs given starting a cell and direction, returning the
        // appropriate score for that 4-cell block
        private static int HeuristicScanCells(BoardCopy board, int row, int col, int rowDirection, int colDirection, Disc aiDisc, Disc opponentDisc, HeuristicWeights weights)
        {
            // set counters to zero
            int aiCount = 0;
            int opponentCount = 0;
            int r = row;
            int c = col;

            // For the starting postion, check the status of 4 discs in a given direction
            for (int i = 0; i < 4; i++)
            {
                Disc cell = board.CellAt(r, c);
                if (cell == aiDisc)
                {
                    aiCount++;
                }
                else if (cell == opponentDisc)
                {
                    opponentCount++;
                }
                r += rowDirection;
                c += colDirection;
            }

            // set an appropriate score based on the set weights
            switch (aiCount, opponentCount)
            {
                case (3, 0): return weights.AiThree;
                case (2, 0): return weights.AiTwo;
                case (1, 0): return weights.AiOne;
                case (0, 3): return weights.OpponentThree;
                case (0, 2): return weights.OpponentTwo;
                case (0, 1): return weights.OpponentOne;
                default: return 0;
            }
        }
    }
}
