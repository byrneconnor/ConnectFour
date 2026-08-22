using ConnectFour.Core;

namespace ConnectFour.AI
{
    // A RandomPlayer 
    public class RandomPlayer : Player
    {
        private readonly Random random = new();

        public RandomPlayer(string name, Disc disc)
            : base(name, disc)
        {
            //
        }

        // IsHuman overwritten to false
        public override bool IsHuman
        {
            get { return false; }
        }

        // Random player computes move here
        public override int GetMove(Board board)
        {
            // Set up a list of possible moves for the AI
            int move;
            List<int> possibleMoves = new();

            // Loop through columns
            for (int c = 0; c < Board.Columns; c++)
            {
                // check if a move is valid
                if (board.IsValidMove(c))
                {
                    // Add to list of possible moves
                    possibleMoves.Add(c);
                }
            }

            // Choose one possible move at random
            move = possibleMoves[random.Next(possibleMoves.Count)];

            return move;
        }
    }
}