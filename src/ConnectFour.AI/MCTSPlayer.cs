using ConnectFour.Core;

namespace ConnectFour.AI
{
    // MCTS - the game is modelled as a tree: each node represents a position reached by a
    // move, and the node stores the move that led to that; it tracks whose turn it is at
    // that stage (and therefore who is next); it tracks the children nodes (following moves
    // from that postion) and which remain unexplored (used in expansion); whether a node is
    // terminal and the result if so.
    public class MCTSPlayer : Player
    {
        private readonly Random random; // use to randomly select moves during expansion
        private Disc aiDisc; // define the AI's disc -needed for searching
        private Disc opponentDisc; // define the opponent's disc - needed for searching
        private readonly int totalIterations; // how many iterations per run
        private const int defaultIterations = 10000; // set default
        private readonly double explorationConstant; // UCB1 exploration weight
        private const double defaultExploration = 1.41421356237; // UCB1 default of sqrt(2)

        public MCTSPlayer(string name, Disc disc, int totalIterations = defaultIterations,
            double explorationConstant = defaultExploration, int? seed = null)
            : base(name, disc)
        {
            // MCTS must have at least 1 iteration set
            if (totalIterations <= 0)
            {
                throw new Exception("totalIterations must be greater than zero.");
            }

            // Exploration constant must be positive
            if (explorationConstant < 0)
            {
                throw new Exception("explorationConstant must be positive.");
            }

            this.totalIterations = totalIterations;
            this.explorationConstant = explorationConstant;
            if (seed == null)
            {
                this.random = new Random();
            }
            else
            {
                this.random = new Random(seed.Value);
            }
        }

        // IsHuman overwritten to false
        public override bool IsHuman
        {
            get { return false; }
        }

        // Class for nodes of the search tree
        private class Node
        {
            public Node? Parent; // parent node (null for root)
            public int Move; // column played to reach this node (-1 for root)
            public Disc DiscJustPlayed; // disc dropped to create this node
            public Disc DiscToMove; // whose turn it is at this node
            public List<Node> Children; // list of children nodes
            public List<int> UntriedMoves; // legal moves not yet expanded
            public bool IsTerminal; // if node is terminal
            public Disc TerminalWinner; // Only useful if terminalNode == true. Disc.Empty means draw
            public int Visits; // number of vistis the node has had during searching
            public double Wins; // number of wins accumulated for DiscJustPlayed

            public Node(Node? parent, int move, Disc discJustPlayed, Disc discToMove,
                        List<int> untriedMoves, bool isTerminal, Disc terminalWinner)
            {
                this.Parent = parent;
                this.Move = move;
                this.DiscJustPlayed = discJustPlayed;
                this.DiscToMove = discToMove;
                this.UntriedMoves = untriedMoves;
                this.Children = new List<Node>(); // start a node with empty list for expand to populate
                this.IsTerminal = isTerminal;
                this.TerminalWinner = terminalWinner;
                this.Visits = 0; // start with 0 for backpropagation to update
                this.Wins = 0; // start with 0 for backpropagation to update
            }

        }

        // GetMove - MCTS player runs n iterations and returns the most visited move
        public override int GetMove(Board board)
        {
            // set the aiDisc and opponentDisc to appropriate colours
            this.aiDisc = this.Colour;
            if (this.aiDisc == Disc.Red)
            {
                this.opponentDisc = Disc.Yellow;
            }
            else
            {
                this.opponentDisc = Disc.Red;
            }

            // Create list of legal moves for board state
            List<int> rootMoves = LegalMoves(new BoardCopy(board));

            // If no moves available, board is full. Shouldn't get to this but throw error if so
            if (rootMoves.Count == 0)
            {
                throw new InvalidOperationException("GetMove called with no legal moves available.");
            }

            // Set up the root node for the MCTS
            Node rootNode = new Node(
                parent: null, 
                move: -1, 
                discJustPlayed: this.opponentDisc, 
                discToMove: this.aiDisc, 
                untriedMoves: rootMoves, 
                isTerminal: false, 
                terminalWinner: Disc.Empty);

            // Start iterations counter
            int i = 0;

            // Loop through iterations
            while (i < this.totalIterations)
            {
                // Each iteration works on a fresh copy of the real position and re-walks the
                // selected path onto it - no per-node board, no clone, no undo.
                BoardCopy boardCopy = new BoardCopy(board);

                // First move - create root node
                Node node = rootNode;

                // Step 1 - Selection: descend until fully expanded, using UCB1 for selection on explored nodes
                // If a non-terminal node has not been explored yet, select that move 
                while (node.IsTerminal == false && node.UntriedMoves.Count == 0 && node.Children.Count > 0)
                {
                    // use UCB to chose child node
                    node = SelectUCBChild(node);
                    // Add move to boadCopy
                    boardCopy.Drop(node.Move, node.DiscJustPlayed);
                }

                // Step 2 - Expansion: add one child node for a node not yet tried
                if (!node.IsTerminal && node.UntriedMoves.Count > 0)
                {
                    node = Expand(node, boardCopy);
                }

                // Step 3 - Simulation: carry out a random rollout or a terminal leaf result
                Disc winner;

                // if terminal, give winner
                if (node.IsTerminal)
                {
                    winner = node.TerminalWinner;
                } 
                // otherwise run a rollout to find a winner
                else
                {
                    winner = Rollout(boardCopy, node.DiscToMove);
                }

                // Step 4 - Backpropagation: send winner information to root
                Backpropagation(node, winner);

                // increase number of iterations by 1
                i++;

            }

            // Return the most visited child (rather than highest win rate, more robust)
            // Ties broken with random selection
            return BestMove(rootNode);

        }
        

        // The legal columns for a given position.
        private static List<int> LegalMoves(BoardCopy board)
        {
            List<int> moves = new List<int>(Board.Columns);
            for (int c = 0; c < Board.Columns; c++)
            {
                if (board.CanPlay(c))
                {
                    moves.Add(c);
                }
            }
            return moves;
        }

        // UCB1 selection 
        private Node SelectUCBChild(Node node)
        {
            // set up a best node variable to be assigned
            Node? best = null;
            // set -Inf as the value to beat
            double bestValue = double.NegativeInfinity;
            // part of the exploration side of UCB1
            double logParentVisits = Math.Log(node.Visits);

            // loop through each of the children nodes
            foreach (Node child in node.Children)
            {
                // calculate UCB formula
                double exploit = child.Wins / child.Visits;
                double explore = this.explorationConstant * Math.Sqrt(logParentVisits / child.Visits);
                double ucb = exploit + explore;

                // reset bestValue based on best UCB found
                if (ucb > bestValue)
                {
                    bestValue = ucb;
                    best = child;
                }
            }

            // return best UCB value
            return best!;
        }

        // Expand the search tree with the child node for a node that has yet to be tried
        // Add that node to the parent node
        private Node Expand(Node node, BoardCopy boardCopy)
        {
            // assign discs
            Disc discJustPlayed = node.DiscJustPlayed;
            Disc discToMove = node.DiscToMove;

            // select one of the node's untried moves randomly. Remove chosen one from untried list
            int randomChoice = this.random.Next(node.UntriedMoves.Count);
            int move = node.UntriedMoves[randomChoice];
            node.UntriedMoves.RemoveAt(randomChoice);
            
            // make the chosen random move
            int row = boardCopy.Drop(move, discToMove);

            // Check if new position is terminal...
            bool isTerminal;
            Disc terminalWinner;
            // First, check if the move a winning move for the players turn
            if (boardCopy.IsWinningMove(row, move, discToMove))
            {
                isTerminal = true;
                terminalWinner = discToMove;
            }
            // Now check if the board is full (draw)
            else if (boardCopy.IsFull())
            {
                isTerminal = true;
                terminalWinner = Disc.Empty;
            }
            else
            {
                isTerminal = false;
                terminalWinner = Disc.Empty;
            }

            // Add the node to the tree
            Node childNode = new Node(
                parent: node, 
                move: move, 
                discJustPlayed: discToMove, // now the next player's turn
                discToMove: discJustPlayed, // and the player just player must wait
                untriedMoves: LegalMoves(boardCopy), 
                isTerminal: isTerminal, 
                terminalWinner: terminalWinner
                );

            // Add child to current node
            node.Children.Add(childNode);

            return childNode;
        }

        // Method to simulate a rollout in step 3 - returns the winning disc (or empty for a draw)
        private Disc Rollout(BoardCopy boardCopy, Disc discToMove)
        {
            // Set a current disc, starting with discToMove
            Disc currentDisc = discToMove;

            // play until a winner is returned
            while (true)
            {
                // Define legal moves
                List<int> moves = LegalMoves(boardCopy);
                
                // if there are no moves left, board is full (draw)
                if (moves.Count == 0)
                {
                    return Disc.Empty; // no winning disc
                }

                // MAke a random move
                int move = moves[this.random.Next(moves.Count)];
                int row = boardCopy.Drop(move, currentDisc);

                // Check if that is a winning move for the current player
                if (boardCopy.IsWinningMove(row, move, currentDisc))
                {
                    return currentDisc;
                }

                // Swap currentDisc value - repeat until a winner is returned
                if (currentDisc == Disc.Red)
                {
                    currentDisc = Disc.Yellow;
                }
                else
                {
                    currentDisc = Disc.Red;
                }
            }

        }

        private void Backpropagation(Node node, Disc winner)
        {
            // start from the leaf reached this iteration (the expanded child, or a terminal node)
            Node? currentNode = node;

            // Loop until currentNode is null, meaning we are back to root node
            while (currentNode != null)
            {
                // Update each nodes visit code and win reward
                currentNode.Visits++;
                currentNode.Wins += Reward(currentNode.DiscJustPlayed, winner);

                // Update currentNode
                currentNode = currentNode.Parent;
            }
        }

        // Set reward for the player who made the move into a node (win = 1, draw = 0.5, loss = 0)
        private static double Reward(Disc discJustPlayed, Disc winner)
        {
            // draw
            if (winner == Disc.Empty)
            {
                return 0.5;
            }
            // win
            else if (winner == discJustPlayed)
            {
                return 1;
            }
            // loss
            else
            {
                return 0;
            }
        }

        // Return the best move for the live game based on 
        private int BestMove(Node rootNode)
        {
            // Initialise an impoosibly low vistScore to start with, all visit values will beat it
            int visitScore = -1;

            // Store a list of moves
            List<int> bestMoves = new List<int>();

            // loop through
            foreach (Node childNode in rootNode.Children)
            {
                // if score is better than current visitScore, replace it and
                // wipe list of moves before adding the new best move to the list
                if (childNode.Visits > visitScore)
                {
                    visitScore = childNode.Visits;
                    bestMoves.Clear();
                    bestMoves.Add(childNode.Move);
                }
                // else if score is as good as existing best, add it to list of moves to take
                else if (childNode.Visits == visitScore)
                {
                    bestMoves.Add(childNode.Move);
                }
            }

            // randomly choose best move if more than one move to choose from
            // otherwise just only move
            return bestMoves[this.random.Next(bestMoves.Count)];

        }

    }
}