using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectFour.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Player playerOne = new Player("Human", "Red");
            Player playerTwo = new Player("AI", "Yellow");

            Console.WriteLine("Player 1: " + playerOne.Name + " - " + playerOne.Disc);
            Console.WriteLine("Player 2: " + playerTwo.Name + " - " + playerTwo.Disc);
        }

    }
}
