using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectFour.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Player playerOne = new Player("Human", Disc.Red);
            Player playerTwo = new Player("AI", Disc.Yellow);
            Player noPlayer = new Player("...", Disc.Empty);

            Console.WriteLine("Player 1: " + playerOne.Name + " - " + playerOne.Colour);
            Console.WriteLine("Player 2: " + playerTwo.Name + " - " + playerTwo.Colour);
            Console.WriteLine("Blank player: " + noPlayer.Name + " - " + noPlayer.Colour);

            Board board = new Board();

            bool validMove = board.IsValidMove(0);
            bool invalidMove = board.IsValidMove(7);

            Console.WriteLine("validMove = " + validMove + ", invalidMove = " + invalidMove);

            // test we can drop some discs and print board out
            Console.WriteLine("Red to column 3...");
            board.DropDisc(3, Disc.Red);
            PrintBoard(board);
            Console.WriteLine("Yellow to column 3...");
            board.DropDisc(3, Disc.Yellow);
            PrintBoard(board);
            Console.WriteLine("Red to column 4...");
            board.DropDisc(4, Disc.Red);
            PrintBoard(board);
            Console.WriteLine("Yellow to column 400 (shouldn't work)...");
            board.DropDisc(400, Disc.Yellow);
            PrintBoard(board);

            Console.WriteLine("Has Red won yet...? - " + board.CheckWin(Disc.Red));
            Console.WriteLine("Add some more goes...Red to coulmn 4 three times...");
            board.DropDisc(4, Disc.Red);
            board.DropDisc(4, Disc.Red);
            board.DropDisc(4, Disc.Red);
            Console.WriteLine("Has Red won yet...? - " + board.CheckWin(Disc.Red));
            PrintBoard(board);

            // Play a game
            Console.WriteLine("Play a game...");
            Game game = new(playerOne, playerTwo);
            Console.WriteLine("Turn 1 - PlayerOne: " + playerOne.Colour + " - Column 3");
            game.PlayMove(3);
            PrintBoard(game.Board);
            Console.WriteLine("Turn 2 - PlayerTwo: " + playerTwo.Colour + " - Column 4");
            game.PlayMove(4);
            PrintBoard(game.Board);
            Console.WriteLine("Turn 3 - PlayerOne: " + playerOne.Colour + " - Column 4");
            game.PlayMove(4);
            PrintBoard(game.Board);
            Console.WriteLine("Turn 4 - PlayerTwo: " + playerTwo.Colour + " - Column 2");
            game.PlayMove(2);
            PrintBoard(game.Board);



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
    }
}
