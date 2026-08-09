namespace ConnectFour.Core
{
    // Game class: requires a board, two players and which of those is the current player
    public class Game
    {
        private readonly Board board = new(); // fresh board inialised
        private readonly Player playerOne;
        private readonly Player playerTwo;
        private Player current;
        private Player? winner;
        private bool isOver;

        public Game(Player playerOne, Player playerTwo)
        {
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            this.current = playerOne;   // Game starts with player 1
            this.winner = null; // Game starts with no winner
            this.isOver = false;
        }

        // get board
        public Board Board
        {
            get { return this.board; }
        }

        // get current player
        public Player CurrentPlayer
        {
            get { return this.current; }
        }

        // get indication if game is over or not
        public bool IsOver
        {
            get { return this.isOver; }
        }

        // the winning player, or null if the game is drawn or still in progress
        // (question mark in front of Player makes it nullable)
        public Player? Winner
        {
            get { return this.winner; }
        }

        // Check move is valid, play turn and then swap players
        public bool PlayMove(int column)
        {
            // Check if game is over
            if (this.isOver)
            {
                return false;
            }

            // Check if the column selected is valid. If not, current player tries again
            if (!this.board.IsValidMove(column))
            {
                return false;
            }

            // Move was valid - drop the disc
            this.board.DropDisc(column, this.current.Colour);

            // checks after move - first if the player has won
            if (this.board.CheckWin(this.current.Colour))
            {
                // if so, set winner to current player and isOver to true
                this.winner = this.current;
                this.isOver = true;
            }
            // If not, check if the board is full and therefore a draw 
            else if (this.board.IsFull())
            {
                // no winners for a draw but isOver set to true
                this.winner = null;
                this.isOver = true;
            }
            else
            // Otherwise it is next player's turn - swap turns
            {
                if (this.current == this.playerOne)
                {
                    this.current = this.playerTwo;
                }
                else
                {
                    this.current = this.playerOne;
                }
            }

            return true;
        }
    }
}