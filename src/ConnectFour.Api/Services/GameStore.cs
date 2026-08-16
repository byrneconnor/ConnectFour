using System.Collections.Concurrent;

using ConnectFour.Core;
using ConnectFour.AI;

namespace ConnectFour.Api.Services
{
    // Represents one game session.
    public class GameSession
    {
        private Guid id; // unique id for a game
        private Game game;
        private Player playerOne;
        private Player playerTwo;


        // Constructor
        public GameSession(
            Guid id,
            Game game,
            Player playerOne,
            Player playerTwo)
        {
            this.id = id;
            this.game = game;
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
        }


        // Get methods
        public Guid GetId()
        {
            return id;
        }

        public Game GetGame()
        {
            return game;
        }

        public Player GetPlayerOne()
        {
            return playerOne;
        }

        public Player GetPlayerTwo()
        {
            return playerTwo;
        }
    }


    // Stores all the active game sessions
    public class GameStore
    {
        // Dictionary containing all game sessions with their unique ids
        private readonly ConcurrentDictionary<Guid, GameSession> sessions = new();


        // Creates a new game session
        public GameSession Create()
        {
            // Create human player
            Player human = new HumanPlayer("Human", Disc.Red);

            // Create AI player
            Player ai = new RandomPlayer("AI", Disc.Yellow);

            // Create game
            var game = new Game(human, ai);

            // Create unique ID for this game
            Guid id = Guid.NewGuid();

            // Create the game session
            var session = new GameSession(
                id,
                game,
                human,
                ai);

            // Store the session using its ID as the key
            sessions[id] = session;

            // Return this session
            return session;
        }


        // Looks for a game session using id
        public GameSession? Get(Guid id)
        {
            // if session found, return it
            if (sessions.TryGetValue(id, out var session))
            {
                return session;
            }
            // if not, return null
            else
            {
                return null;
            }
        }
    }
}