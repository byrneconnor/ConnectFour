using ConnectFour.Api.Services;
using ConnectFour.Core;

namespace ConnectFour.Api.Dtos
{
    // Represents a request to make a move
    public class MoveRequest
    {
        public int Column { get; set; }
    }


    // Represents the current state of a game that gets sent to the frontend
    public class GameStateDto
    {
        public Guid Id { get; }
        public string[][] Board { get; }
        public string CurrentPlayer { get; }
        public string CurrentColour { get; }
        public bool IsOver { get; }
        public string? Winner { get; }
        public bool IsDraw { get; }
        public int[] ValidMoves { get; }

        // Constructor
        public GameStateDto(
            Guid id,
            string[][] board,
            string currentPlayer,
            string currentColour,
            bool isOver,
            string? winner,
            bool isDraw,
            int[] validMoves)
        {
            Id = id;
            Board = board;
            CurrentPlayer = currentPlayer;
            CurrentColour = currentColour;
            IsOver = isOver;
            Winner = winner;
            IsDraw = isDraw;
            ValidMoves = validMoves;
        }
    }


    // GameMapper converts the internal GameSession object into a GameStateDto to send to frontend
    public static class GameMapper
    {
        public static GameStateDto ToDto(GameSession session)
        {
            // Get the actual game from the session
            var game = session.GetGame();

            // Get the board from the game
            var board = game.Board;


            // Create a 2D array of strings to represent the board
            var cells = new string[Board.Rows][];

            // Loop through each row and create an array
            for (int r = 0; r < Board.Rows; r++)
            {
                cells[r] = new string[Board.Columns];

                // Loop through every column in the row and convert cell into string
                for (int c = 0; c < Board.Columns; c++)
                {
                    cells[r][c] = board.CellAt(r, c).ToString();
                }
            }

            // Create a list to hold the columns where the player is currently allowed to play
            var valid = new List<int>();

            // Check every column on the board
            for (int c = 0; c < Board.Columns; c++)
            {
                // If move is valid, add column to list of valid moves
                if (board.IsValidMove(c))
                {
                    valid.Add(c);
                }
            }

            // Create and return a GameStateDto containing all the information the frontend needs
            return new GameStateDto(
                session.GetId(),
                cells,
                game.CurrentPlayer.Name,
                game.CurrentPlayer.Colour.ToString(),
                game.IsOver,
                game.Winner?.Name,
                game.IsOver && game.Winner is null,
                valid.ToArray());
        }
    }
}