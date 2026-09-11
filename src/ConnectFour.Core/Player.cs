namespace ConnectFour.Core
{
    public abstract class Player
    {
        private string name;
        private Disc colour;

        protected Player(string name, Disc colour)
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

        // method to flag for human or AI players
        public abstract bool IsHuman { 
            get; 
        }

        // For non-human players only so they can compute a move themselves
        public abstract int GetMove(Board board);

        // option metric used for evaluating nodes searched through minimax
        // Set it to null within Player to be overridden by MinimaxPlayer
        public virtual long? GetNodesSearched
        {
            get { return null; }
        }

    }
}