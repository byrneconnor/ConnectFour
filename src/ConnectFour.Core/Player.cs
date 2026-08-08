using ConnectFour.Core;

namespace ConnectFour
{
    public class Player
    {
        private string name;
        private string disc;

        public Player(string name, string disc)
        {
            this.name = name;
            this.disc = disc;
        }

        public string Name
        {
            set { this.name = value; }
            get { return this.name; }
        }

        public string Disc
        {
            set { this.disc = value; }
            get { return this.disc; }
        }

    }
}