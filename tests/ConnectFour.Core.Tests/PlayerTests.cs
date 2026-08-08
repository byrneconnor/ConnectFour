namespace ConnectFour.Core.Tests
{
    public class PlayerTests
    {
        // Test name set correctly
        [Fact]
        public void Player_SetsName()
        {
            var player = new Player("playerOne", Disc.Red);

            Assert.Equal("playerOne", player.Name);
        }

        // Test disc set correctly
        [Fact]
        public void Player_SetsDisc()
        {
            var player = new Player("playerOne", Disc.Red);

            Assert.Equal(Disc.Red, player.Colour);
        }

        // Test both name and disc set correctly
        [Fact]
        public void Player_SetsBothIndependently()
        {
            var player = new Player("playerTwo", Disc.Yellow);

            Assert.Equal("playerTwo", player.Name);
            Assert.Equal(Disc.Yellow, player.Colour);
        }

        // Check name gets updated
        [Fact]
        public void Player_UpdateName()
        {
            var player = new Player("playerOne", Disc.Red);

            player.Name = "playerTwo";

            Assert.Equal("playerTwo", player.Name);
        }

        // Check name update doesn't affect disc
        [Fact]
        public void Player_DiscRemainsSame()
        {
            var player = new Player("playerOne", Disc.Red);

            player.Name = "playerTwo";

            Assert.Equal(Disc.Red, player.Colour);
        }

        // Check disc gets updated
        [Fact]
        public void Player_UpdateDisc()
        {
            var player = new Player("playerOne", Disc.Red);

            player.Colour = Disc.Yellow;

            Assert.Equal(Disc.Yellow, player.Colour);
        }

        // Check disc update doesn't affect name
        [Fact]
        public void Player_NameRemainsSame()
        {
            var player = new Player("playerOne", Disc.Red);

            player.Colour = Disc.Yellow;

            Assert.Equal("playerOne", player.Name);
        }

        // Name is still a reference type, so null is allowed
        [Fact]
        public void Player_AllowsNullName()
        {
            var player = new Player(null, Disc.Red);

            Assert.Null(player.Name);
        }

        // Test multiple name/disc combinations (enums are valid in InlineData)
        [Theory]
        [InlineData("", Disc.Empty)]
        [InlineData("playerOne", Disc.Yellow)]
        [InlineData("playerTwo", Disc.Red)]
        public void Player_PreservesValues(string name, Disc disc)
        {
            var player = new Player(name, disc);

            Assert.Equal(name, player.Name);
            Assert.Equal(disc, player.Colour);
        }

        // Name can be reassigned to null
        [Fact]
        public void Player_ReassignNullName()
        {
            var player = new Player("playerOne", Disc.Red);

            player.Name = null;

            Assert.Null(player.Name);
        }
    }
}
