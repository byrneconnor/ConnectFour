namespace ConnectFour.Core.Tests
{
    public class PlayerTests
    {
        // Test name set correctly
        [Fact]
        public void Player_SetsName()
        {
            var player = new Player("playerOne", "Red");

            Assert.Equal("playerOne", player.Name);
        }

        // Test disc set correctly
        [Fact]
        public void Player_SetsDisc()
        {
            var player = new Player("playerOne", "Red");

            Assert.Equal("Red", player.Disc);
        }

        // Test both name and disc set correctly
        [Fact]
        public void Player_SetsBothIndependently()
        {
            var player = new Player("playerTwo", "Yellow");

            Assert.Equal("playerTwo", player.Name);
            Assert.Equal("Yellow", player.Disc);
        }

        // Check name gets updated
        [Fact]
        public void Player_UpdateName()
        {
            var player = new Player("playerOne", "Red");

            player.Name = "playerTwo";

            Assert.Equal("playerTwo", player.Name);
        }

        // Check name update doesn't affect disc
        [Fact]
        public void Player_DiscRemainsSame()
        {
            var player = new Player("playerOne", "Red");

            player.Name = "playerTwo";

            Assert.Equal("Red", player.Disc);
        }

        // Check disc gets updated
        [Fact]
        public void Player_UpdateDisc()
        {
            var player = new Player("playerOne", "Red");

            player.Disc = "Yellow";

            Assert.Equal("Yellow", player.Disc);
        }

        // Check disc update doesn't affect name
        [Fact]
        public void Player_NameRemainsSame()
        {
            var player = new Player("playerOne", "Red");

            player.Disc = "Yellow";

            Assert.Equal("playerOne", player.Name);
        }

        // Check player accepts null values
        [Fact]
        public void Player_AllowsNullValues()
        {
            var player = new Player(null, null);

            Assert.Null(player.Name);
            Assert.Null(player.Disc);
        }

        // Test multiple strings
        [Theory]
        [InlineData("", "")]
        [InlineData("playerOne", "Yellow")]
        [InlineData("playerTwo", "Red")]
        public void Player_PreservesVariousStrings(string name, string disc)
        {
            var player = new Player(name, disc);

            Assert.Equal(name, player.Name);
            Assert.Equal(disc, player.Disc);
        }

        // Check we can reassign nulls
        [Fact]
        public void Player_ReassignNulls()
        {
            var player = new Player("playerOne", "Red");

            player.Name = null;
            player.Disc = null;

            Assert.Null(player.Name);
            Assert.Null(player.Disc);
        }
    }
}
