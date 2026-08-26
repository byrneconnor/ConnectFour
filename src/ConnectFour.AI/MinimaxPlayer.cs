using ConnectFour.Core;

namespace ConnectFour.AI
{
    // The Minimax opponent
    public class MinimaxPlayer : Player
    {
        private readonly Random random = new(); // use to randomly select one of the best moves
        private Disc aiDisc; // define the AI's disc -needed for searching
        private Disc opponentDisc; // define the opponent's disc - needed for searching
        private static readonly int[] ColumnOrder = { 3, 4, 2, 5, 1, 6, 0 }; // order to play columns, helps to speed up alpha-beta pruning by playing better columns first
        private readonly int searchDepth; // Set search depth for minimax (so it doesn't search all the way to terminal nodes)
        private const int DefaultDepth = 8; // set default for now
        private const int WinScore = 1000000; // large enough that any real win/loss outranks every heuristic score
        private readonly HeuristicWeights weights; // weights for heuristic evaluation to produce scores at non-terminal nodes

        public MinimaxPlayer(string name, Disc disc, int searchDepth = DefaultDepth, HeuristicWeights? weights = null)
            : base(name, disc)
        {
            this.searchDepth = searchDepth;
            this.weights = weights ?? new HeuristicWeights();
        }

        // IsHuman overwritten to false
        public override bool IsHuman
        {
            get { return false; }
        }

        // GetMove - Minimax player returns the chosen move
        public override int GetMove(Board board)
        {
            // set the aiDisc and opponentDisc to appropriate colours
            this.aiDisc = this.Colour; 
            if (this.aiDisc == Disc.Red)
            {
                this.opponentDisc = Disc.Yellow;
            } else
            {
                this.opponentDisc = Disc.Red;
            }

            // Create a clone of current board
            BoardCopy boardCopy = new BoardCopy(board);

            // For the AI (maximiser), set a large minimum value
            int bestScore = int.MinValue;

            // Create bestMoves: a list of column(s) with the best score
            List<int> bestMoves = new List<int>();

            // Loop through each column and use minimax to return the best move
            foreach (int col in ColumnOrder)
            {
                // Create a variable to store the score
                int score;

                // Check if a column can be played
                if (!boardCopy.CanPlay(col))
                {
                    continue;
                }

                // Drop the disc in the column and return the row to use to check win
                int row = boardCopy.Drop(col, this.aiDisc);

                // Update the score, first by seeing if there has been a win                
                if (boardCopy.IsWinningMove(row, col, this.aiDisc))
                {
                    score = WinScore; 
                }
                // then check if there is a draw
                else if (boardCopy.IsFull())
                {
                    score = 0;                  
                }
                // Otherwise, recursively play out each possible game to get the best possible score
                else
                {
                    // pass in search depth at 1 (to include this node search) and the starting values for alpha and beta
                    score = Minimax(boardCopy, this.opponentDisc, 1, int.MinValue, int.MaxValue);
                }

                // Return the board clone back to the original state
                boardCopy.Undo(col);

                // If current score is better than the previous bestScore...
                if (score > bestScore)
                {
                    // Update the bestScore for future comparisons
                    bestScore = score;
                    // Wipe bestMoves and add column to bestMoves
                    bestMoves.Clear();
                    bestMoves.Add(col);
                }
                // if score is as good as current bestScore, add column to the list of bestMoves
                else if (score == bestScore)
                {
                    bestMoves.Add(col);
                }
            }

            // would suggest board is full, should not happen but raise an error if it does
            if (bestMoves.Count == 0) 
            {
                throw new InvalidOperationException("GetMove called with no legal moves available.");
            }

            // for multiple best columns, randomly select a cloumn to play
            return bestMoves[this.random.Next(bestMoves.Count)];
        }



        // Minimax - recursively play out all possible games. Each iteration takes 
        // a copy of the board for each move and which disc to move (plays out both
        // the AI/maximiser and the opponent/minimiser).
        // We add the alpha and beta variables to allow pruning to take place
        // The result will be the same but we get there faster
        private int Minimax(BoardCopy boardCopy, Disc discToMove, int depth, int alpha, int beta)
        {
            // Reached the search depth limit without a terminal node, so return a static heuristic
            // estimate instead of searching deeper
            if (depth >= this.searchDepth)
            {
                return Heuristics.HeuristicEvaluation(boardCopy, this.aiDisc, this.opponentDisc, this.weights);
            }

            // Check if this turn is for the maximiser to determine scores to set
            bool maximiserTurn = (discToMove == this.aiDisc);

            // Set the next disc for the next game and set value for current game
            // to maximum/minimiser dependent on whose turn it is
            Disc nextDisc;
            int value;
            if (maximiserTurn)
            {
                nextDisc = this.opponentDisc; // next turn after this is opponent's 
                value = int.MinValue; // AI is maximiser, so set value extremely low
            } else
            {
                nextDisc = this.aiDisc; // next turn after this is AI's
                value = int.MaxValue; // Human is minimiser, so set value extremely high
            }

            // Loop through each column, return the best score for the player
            foreach (int col in ColumnOrder)
            {
                // Create a variable to store the score
                int score;

                // Check the column can be played
                if (!boardCopy.CanPlay(col))
                {
                    continue;
                }

                // Drop the disc in the column and return the row to use to check win
                int row = boardCopy.Drop(col, discToMove);

                // Update the score, first by seeing if there has been a win for current player
                if (boardCopy.IsWinningMove(row, col, discToMove))
                {
                    // Update score based on player
                    if (maximiserTurn)
                    {
                        // Prioritise earlier winning moves rather then a win that takes longer to get
                        score = WinScore - depth; 
                    } else
                    {
                        score = -(WinScore - depth);
                    }
                }
                // Check if the game is a draw
                else if (boardCopy.IsFull())
                {
                    score = 0;
                }
                // Otherwise, recursively play out games and return best score
                else
                {
                    score = Minimax(boardCopy, nextDisc, depth + 1, alpha, beta);
                }

                boardCopy.Undo(col);

                if (maximiserTurn)
                {
                    // For the maximiser, if score is higher than current score, update value
                    if (score > value)
                    {
                        value = score;
                    }
                    // and if that score is greater than current alpha, update
                    if (value > alpha)
                    {
                        alpha = value;
                    }
                } 
                else 
                {
                    // for minimiser, if score is lower than current score, update value
                    if (score < value)
                    {
                        value = score;
                    }
                    // and if that score is lower than current beta, update
                    if (value < beta)
                    {
                        beta = value;
                    }

                }

                // if alpha is greater or equal to beta, prune this branch
                if (alpha >= beta)
                {
                    break;
                }

            }

            return value;
        }
    }
}