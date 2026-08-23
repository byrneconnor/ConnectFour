using System.Collections.ObjectModel;

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
            public int CentreDisc { get; set; } = 3;

        }

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
                return boardCopy.HeuristicEvaluation(this.aiDisc, this.opponentDisc, this.weights);
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

        // BoardCopy - a copy of a board that can be used for the minimax search
        private class BoardCopy
        {
            private readonly Disc[,] cells = new Disc[Board.Rows, Board.Columns]; // the board grid to copy
            private readonly int[] heights = new int[Board.Columns]; // array of heights to quickly determine top available row

            // Copy the current board state - loop each cell and fill it respectively
            public BoardCopy(Board board)
            {
                for (int c = 0; c < Board.Columns; c++)
                {
                    // track how much the column is full
                    int filled = 0;
                    for (int r = 0; r < Board.Rows; r++)
                    {
                        Disc d = board.CellAt(r, c);
                        this.cells[r, c] = d;
                        if (d != Disc.Empty) filled++;
                    }
                    // fill heights with each colum's height
                    this.heights[c] = filled;
                }
            }

            // Check if a column can be played using heights
            public bool CanPlay(int col)
            {
                return this.heights[col] < Board.Rows;
            }

            // Check if board is full - can any column be played?
            public bool IsFull()
            {
                for (int c = 0; c < Board.Columns; c++)
                {
                    if (CanPlay(c)) return false;  
                }
                return true;
            }

            // Drop a disc into a column using the heights array for quickly finding
            // next avaiable row. 
            public int Drop(int col, Disc disc)
            {
                // Find top cell available
                int row = Board.Rows - 1 - this.heights[col];
                // Add disc
                this.cells[row, col] = disc;
                // Update heights
                this.heights[col]++;
                // Returns the row value just filled to get used for checking for a win
                return row;
            }

            // Undo the last row filled in a particular column. Used by the minimax search
            public void Undo(int col)
            {
                // Amend heights
                this.heights[col]--;
                // Find the row value to be undone
                int row = Board.Rows - 1 - this.heights[col];
                // Change cell to empty
                this.cells[row, col] = Disc.Empty;
            }

            // Return the number of adjacent cells matching the disc colur in a
            // particular direction (set by rowDirection, colDirection) starting from 
            // a particular cell (set by row, col)
            private int ScanCells(int row, int col, int rowDirection, int colDirection, Disc disc)
            {
                // Set a counter
                int count = 0;
                // set up row of current cell to scan
                int r = row + rowDirection;
                // set up column of current cell to scan
                int c = col + colDirection;
                // while the cell to scan remains in the grid boundaries and matches disc colur
                while (r >= 0 && r < Board.Rows && c >= 0 && c < Board.Columns
                       && this.cells[r, c] == disc)
                {
                    // add one to the counter
                    count++;
                    // move the row index to the next cell
                    r += rowDirection;
                    // move the column index to the next cell
                    c += colDirection;
                }
                return count;
            }

            // Tally up how many matching discs for the current disc were found
            private int CheckForLine(int row, int col, int rowDirection, int colDirection, Disc disc)
            {
                // 1 for the cell just played, count for one direction and count for the direct opposite direction
                return 1 + ScanCells(row, col, rowDirection, colDirection, disc) 
                         + ScanCells(row, col, -rowDirection, -colDirection, disc);
            }

            // Check for a winning move on latest move (not the whole board)
            public bool IsWinningMove(int row, int col, Disc disc)
            {
                return CheckForLine(row, col, 0, 1, disc) >= 4 // horizontal
                    || CheckForLine(row, col, 1, 0, disc) >= 4 // vertical
                    || CheckForLine(row, col, 1, 1, disc) >= 4 // top left to bottom right (\)
                    || CheckForLine(row, col, 1, -1, disc) >= 4; // bottom left to top right (/)
            }

            // Heuristic evaluation for non-terminal nodes - scan the board to score cells that
            // look more promising
            public int HeuristicEvaluation(Disc aiDisc, Disc opponentDisc, HeuristicWeights weights)
            {
                // Set a score
                int score = 0;

                // Reward the AI (and penalise the opponent) for holding cells in centre column
                int centre = Board.Columns / 2;
                for (int r = 0; r < Board.Rows; r++)
                {
                    if (this.cells[r, centre] == aiDisc) score += weights.CentreDisc;
                    else if (this.cells[r, centre] == opponentDisc) score -= weights.CentreDisc;
                }

                // Check all horizontal rows and update score
                for (int r = 0; r < Board.Rows; r++)
                {
                    for (int c = 0; c <= Board.Columns - 4; c++)
                    {
                        score += HeuristicScanCells(r, c, 0, 1, aiDisc, opponentDisc, weights);
                    }
                }

                // Check for all vertical windows and update score
                for (int r = 0; r <= Board.Rows - 4; r++)
                {
                    for (int c = 0; c < Board.Columns; c++)
                    {
                        score += HeuristicScanCells(r, c, 1, 0, aiDisc, opponentDisc, weights);
                    }
                }

                // Check for all top left to bottom right (\) and update score
                for (int r = 0; r <= Board.Rows - 4; r++)
                {
                    for (int c = 0; c <= Board.Columns - 4; c++)
                    {
                        score += HeuristicScanCells(r, c, 1, 1, aiDisc, opponentDisc, weights);
                    }
                }

                // Check for all bottom left to top right (/) and update score
                for (int r = 3; r < Board.Rows; r++)
                {
                    for (int c = 0; c <= Board.Columns - 4; c++)
                    {
                        score += HeuristicScanCells(r, c, -1, 1, aiDisc, opponentDisc, weights);
                    }
                }

                return score;
            }

            // Track discs given starting a cell and direction, returning the
            // appropriate score for that 4-cell block
            private int HeuristicScanCells(int row, int col, int rowDirection, int colDirection, Disc aiDisc, Disc opponentDisc, HeuristicWeights weights)
            {
                // set counters to zero
                int aiCount = 0;
                int opponentCount = 0;
                int r = row;
                int c = col;
                
                // For the starting postion, check the status of 4 discs in a given direction
                for (int i = 0; i < 4; i++)
                {
                    Disc cell = this.cells[r, c];
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
}