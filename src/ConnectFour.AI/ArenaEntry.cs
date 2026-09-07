namespace ConnectFour.AI
{
    // An arena competitor: a display name plus a factory that builds a fresh,
    // coloured, seeded player for one game. Same CreatePlayer delegate the
    // benchmark uses - the arena just pairs it with a name because it runs
    // many competitors at once.
    public record ArenaEntry(string Name, PlayerFactory Create);
}

