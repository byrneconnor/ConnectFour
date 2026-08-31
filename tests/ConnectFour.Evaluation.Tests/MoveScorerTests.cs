using ConnectFour.Core;

namespace ConnectFour.Evaluation.Tests
{
    public class MoveScorerTests
    {
        SolvedPosition testCaseOne = new SolvedPosition(
                Position: "32751571231557",
                DiscToMove: Disc.Red,
                ColumnScores: [-4, -3, -3, -5, -5, -9, -4],
                BestColumns: [1, 2],
                Value: -3,
                Outcome: "Loss",
                ExpectedValue: -3,
                MatchesExpected: true,
                SourceFile: "Test_L1_R2.txt");

        SolvedPosition testCaseTwo = new SolvedPosition(
                Position: "3457741246677474572223453551",
                DiscToMove: Disc.Red,
                ColumnScores: [-7, -7, 5, null, -7, -7, null],
                BestColumns: [2],
                Value: 5,
                Outcome: "Win",
                ExpectedValue: 5,
                MatchesExpected: true,
                SourceFile: "Test_L2_R1.txt");

        SolvedPosition testCaseThree = new SolvedPosition(
                Position: "32114125125351447166362752355326644364777",
                DiscToMove: Disc.Yellow,
                ColumnScores: [null, null, null, null, null, null, 0],
                BestColumns: [6],
                Value: 0,
                Outcome: "Draw",
                ExpectedValue: 0,
                MatchesExpected: true,
                SourceFile: "Test_L3_R1.txt");


        [Fact]
        public void MoveScorer_AgreementAndZeroRegret()
        {
            MoveScore result = MoveScorer.Score(testCaseTwo, 2);

            Assert.True(result.Agreement);
            Assert.Equal(0, result.Regret);
            Assert.True(result.ResultPreserved);
            Assert.Equal(0, result.SpeedRegret);
        }


        [Fact]
        public void MoveScorer_ResultPreservedWithRegret()
        {
            MoveScore result = MoveScorer.Score(testCaseOne, 0);

            Assert.False(result.Agreement);
            Assert.Equal(1, result.Regret);
            Assert.True(result.ResultPreserved);
            Assert.Equal(1, result.SpeedRegret);
        }


        [Fact]
        public void MoveScorer_LosingMoveDoesNotPreserveResult()
        {
            MoveScore result = MoveScorer.Score(testCaseTwo, 1);

            Assert.False(result.Agreement);
            Assert.Equal(12, result.Regret);
            Assert.False(result.ResultPreserved);
            Assert.Null(result.SpeedRegret);
        }


        [Fact]
        public void MoveScorer_FullColumnThrows()
        {
            Assert.Throws<Exception>(
                () => MoveScorer.Score(testCaseTwo, 3));
        }


        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        public void InvalidColumn_Throws(int chosenColumn)
        {
            Assert.Throws<Exception>(
                () => MoveScorer.Score(testCaseOne, chosenColumn));
        }
    }
}