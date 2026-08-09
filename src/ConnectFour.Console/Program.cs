using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectFour.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Define players
            Player human = new Player("Human", Disc.Red);
            Player ai = new Player("AI", Disc.Yellow);

            // Define game
            Game game = new(human, ai);

            Console.WriteLine("Let's play Connect Four!");

            // Print initial blank board
            PrintBoard(game.Board);

            while (game.IsOver == false)
            {
                
                int move;

                // every time it is the human player, ask for an input
                if (game.CurrentPlayer.Name == "Human")
                {
                    move = AskForColumn(game);
                }
                else
                // AI's turn
                {
                    // Set up a list of possible moves for the AI
                    List<int> possibleMoves = new();
                    
                    // Loop through columns
                    for (int c = 0; c < Board.Columns; c++)
                    {
                        // check if a move is valid
                        if (game.Board.IsValidMove(c))
                        {
                            // Add to list of possible moves
                            possibleMoves.Add(c);
                        }
                    }

                    // Choose one possible move at random
                    Random random = new();
                    move = possibleMoves[random.Next(possibleMoves.Count)];
                    Console.WriteLine("AI chooses column " + move);
                }

                // Play respective move, human or AI. Repeat until IsOver == true
                game.PlayMove(move);

                // Print board after each move
                PrintBoard(game.Board);

            }

            // Announce game winner
            if (game.Winner != null)
            {
                Console.WriteLine($"{game.Winner.Name} wins!");
            }
            // Otherwise, announce draw
            else
            {
                Console.WriteLine("Draw!");
            }

        }

        // Print board to console - for testing
        static void PrintBoard(Board board)
        {
            // for each row, print the disc present on each cell
            // R for red, Y for yellow, . for empty
            for (int r = 0; r < Board.Rows; r++)
            {
                Console.Write("| ");
                for (int c = 0; c < Board.Columns; c++)
                {
                    char symbol;
                    switch (board.CellAt(r, c))
                    {
                        case Disc.Red:
                            symbol = 'R';
                            break;
                        case Disc.Yellow:
                            symbol = 'Y';
                            break;
                        default:
                            symbol = '.';
                            break;
                    }
                    Console.Write($"{symbol} ");
                }
                Console.WriteLine("|");
            }
            Console.WriteLine("-----------------");
            Console.WriteLine("  0 1 2 3 4 5 6");
            Console.WriteLine("\n");

        }

        static int AskForColumn(Game game)
        {
            // infinite loop - until we get a valid response
            while (true)
            {
                // Ask human for a column
                Console.Write("Choose column (0-6): ");

                
                // if we get an int and move is valid, return the int for the column
                if (int.TryParse(Console.ReadLine(), out int column))
                {
                    
                    if (game.Board.IsValidMove(column))
                    {
                        return column;
                    }

                }

                // Otherwise, ask again
                Console.WriteLine("Invalid move. Please try again.");
            }
        }

    }
}
