namespace ConnectFour.Core
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(string name, Disc disc)
            : base(name, disc)
        {
        }

        public override bool IsHuman
        {
            get { return true; }
        }

        // A human never computes its own move, it is provided.
        // This method is never called, but an error message
        // will indicate if it gets used
        public override int GetMove(Board board)
        {
            throw new InvalidOperationException(
                "A human player's turn should be provided, not computed!");
        }
    }
}
