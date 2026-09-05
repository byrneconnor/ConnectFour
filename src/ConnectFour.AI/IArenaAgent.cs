using ConnectFour.Core;

namespace ConnectFour.AI
{
    // Contract for arena competitors: a name and a way to hand back a fresh, seeded
    // player for one game set to a given colour. AI agents implement it directly, so
    // each agent is its own factory
    public interface IArenaPlayer
    {
        // Display name used in the results, already satisfied by Player.Name
        string Name { get; }

        // Build a fresh player for one game, playing the disc colour to
        // play as plus the seed for reproducibility
        Player CreatePlayer(Disc colour, int seed);
    }
}