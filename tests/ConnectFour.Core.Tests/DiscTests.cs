using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectFour.Core.Tests
{
    public class DiscTests
    {
        // Empty is the default value (an uninitialised Disc should be Empty)
        [Fact]
        public void Disc_DefaultIsEmpty()
        {
            Disc disc = default;

            Assert.Equal(Disc.Empty, disc);
        }

        // Underlying integer values are as expected
        [Theory]
        [InlineData(Disc.Empty, 0)]
        [InlineData(Disc.Red, 1)]
        [InlineData(Disc.Yellow, 2)]
        public void Disc_HasExpectedUnderlyingValue(Disc disc, int value)
        {
            Assert.Equal(value, (int)disc);
        }

        // Enum defines exactly three members
        [Fact]
        public void Disc_HasThreeValues()
        {
            var values = Enum.GetValues<Disc>();

            Assert.Equal(3, values.Length);
        }

        // Enum contains each expected member
        [Fact]
        public void Disc_ContainsExpectedMembers()
        {
            var values = Enum.GetValues<Disc>();

            Assert.Contains(Disc.Empty, values);
            Assert.Contains(Disc.Red, values);
            Assert.Contains(Disc.Yellow, values);
        }

        // Defined values are recognised, undefined ones are not
        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, false)]
        [InlineData(-1, false)]
        public void Disc_IsDefinedMatchesValidValues(int value, bool expected)
        {
            Assert.Equal(expected, Enum.IsDefined(typeof(Disc), value));
        }

        // Each member converts to its expected name
        [Theory]
        [InlineData(Disc.Empty, "Empty")]
        [InlineData(Disc.Red, "Red")]
        [InlineData(Disc.Yellow, "Yellow")]
        public void Disc_ToStringReturnsName(Disc disc, string name)
        {
            Assert.Equal(name, disc.ToString());
        }

        // Parsing a name returns the matching member
        [Theory]
        [InlineData("Empty", Disc.Empty)]
        [InlineData("Red", Disc.Red)]
        [InlineData("Yellow", Disc.Yellow)]
        public void Disc_ParseReturnsMember(string name, Disc expected)
        {
            var parsed = Enum.Parse<Disc>(name);

            Assert.Equal(expected, parsed);
        }
    }
}
