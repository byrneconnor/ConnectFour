using ConnectFour.Core;

namespace ConnectFour
{
    public class Player
    {
        private string name;
        private Disc colour;

        public Player(string name, Disc colour)
        {
            this.name = name;
            this.colour = colour;
        }

        public string Name
        {
            set { this.name = value; }
            get { return this.name; }
        }

        public Disc Colour
        {
            set { this.colour = value; }
            get { return this.colour; }
        }

    }
}