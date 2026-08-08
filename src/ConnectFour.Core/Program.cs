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
        }

    }
}
