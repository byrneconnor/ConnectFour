using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectFour.Core
{
    // Game class: requires a board, two players and which of those is the current player
    public class Game
    {
        private readonly Board board = new(); // fresh board inialised
        private readonly Player playerOne;
        private readonly Player playerTwo;
        private Player current;
        
        public Game(Player playerOne, Player playerTwo)
        {
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            this.current = playerOne;   // Game starts with player 1
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

        // Check move is valid, play turn and then swap players
        public bool PlayMove(int column)
        {
            // Check if the column selected is valid. If not, current player tries again
            if (!this.board.IsValidMove(column))
            {
                return false;
            }

            // Move was valid - drop the disc
            this.board.DropDisc(column, this.current.Colour);

            // Next player's turn - swap turns
            if (this.current == this.playerOne)
            {
                this.current = this.playerTwo;
            }
            else
            {
                this.current = this.playerOne;
            }

            return true;
        }
    }
}