using System.Text.Json;

using ConnectFour.Core;

namespace ConnectFour.Evaluation.Tests
{
    public class SolverBoardTests
    {
        // Set up some test data
        public sealed record PositionExample(
            string Position,
            Disc SideToMove,
            int[] FullColumns,
            int[] BestColumns);

        public static TheoryData<PositionExample> TestExamples() => new()
        {
            new("77541", Disc.Yellow, [], [3]),
            new("122743", Disc.Red, [], [3]),
            new("32751571231557", Disc.Red, [], [1, 2]),
            new("13232137725333", Disc.Red, [2], [0, 1, 3, 4, 6]),
            new("15127677722767", Disc.Red, [6], [4]),
            new("1233722555341451114725221333", Disc.Red, [0, 1, 2], [3, 4, 5, 6]),
            new("6672375354252731116762237724", Disc.Red, [1, 6], [4, 5]),
        };

        // Read in benchmark data - used later
        //private static readonly string DataPath =
        //    Path.Combine(AppContext.BaseDirectory, "data", "benchmark-positions-scraped.json");

        //// Only the fields the invariants need. JSON keys are camelCase and line up with these names.
        //private sealed record Record(
        //    string position,
        //    string discToMove,
        //    int?[] columnScores,
        //    int[] bestColumns,
        //    int value);

        //// Deserialize once and cache — the file is ~2 MB, so we don't want to re-read it per fact.
        //private static Record[]? cachedRecords;

        //private static Record[] AllRecords()
        //{
        //    if (cachedRecords is not null)
        //    {
        //        return cachedRecords;
        //    }

        //    Assert.True(File.Exists(DataPath),
        //        $"benchmark data not found at {DataPath}. Ensure the .csproj copies it to the output directory.");

        //    JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        //    cachedRecords = JsonSerializer.Deserialize<Record[]>(File.ReadAllText(DataPath), options)!;
        //    return cachedRecords;
        //}

        //// Take a sample portion
        //private const int SampleStride = 10;
        //private static IEnumerable<Record> Sample()
        //{
        //    int i = 0;

        //    foreach (Record record in AllRecords())
        //    {
        //        if (i++ % SampleStride == 0)
        //            yield return record;
        //    }
        //}


        // Check a best column reported by the solver must be playable on the matching engine board.
        [Theory]
        [MemberData(nameof(TestExamples))]
        public void SolverBoard_PossibleToPlayBestColumn(PositionExample e)
        {
            Board board = SolverBoard.SolverStringPositionToBoard(e.Position); 

            Assert.Equal(e.SideToMove, SolvedPosition.SideToMove(e.Position));
            Assert.Equal(e.FullColumns, SolverBoard.FullColumns(board).ToArray());

            // Check best column is a playable move.
            foreach (int c in e.BestColumns)
            {
                Assert.True(board.IsValidMove(c),
                    $"Best column {c} is not playable");
            }
        }

        // Check every other column is not affected by a move
        [Theory]
        [InlineData("77")] 
        [InlineData("4")]  
        public void SolverBoard_CheckDiscDropsCorrectly(string position)
        {
            Board board = SolverBoard.SolverStringPositionToBoard(position);
            int column = position[0] - '1';

            // Check the column played holds the Red disc at the bottom cell
            Assert.Equal(Disc.Red, board.CellAt(Board.Rows - 1, column));
            // Every other column's bottom cell is still empty.
            for (int c = 0; c < Board.Columns; c++)
            {
                if (c != column)
                {
                    Assert.Equal(Disc.Empty, board.CellAt(Board.Rows - 1, c));
                }
            }
        }

        // Check we only accept columns 1 to 7 (0 to 6 in our game logic)
        [Theory]
        [InlineData("8")]   
        [InlineData("0")]   
        [InlineData("abc")] // non-digit
        [InlineData("1 1")] // contains a space
        public void SolverBoard_CheckBadPositions(string position)
        {
            Assert.Throws<FormatException>(() => SolverBoard.SolverStringPositionToBoard(position));
        }

        
        
        //[Fact]
        //public void SolverBoard_EveryPositionHasLegalMove()
        //{
        //    foreach (Record r in Sample())
        //    {
        //        // Throws an error if move not legal
        //        _ = SolverBoard.SolverStringPositionToBoard(r.position);
        //    }
        //}

        //// Disc colour for SideToMove matches the data's record
        //[Fact]
        //public void SideToMove_matches_the_data()
        //{
        //    foreach (Record r in Sample())
        //    {
        //        Assert.Equal(r.discToMove, SolvedPosition.SideToMove(r.position).ToString());
        //    }
        //}

        //// Check any full columns in both data and boards match
        //[Fact]
        //public void SolvedBoard_FullColumnsMatch()
        //{
        //    foreach (Record r in Sample())
        //    {
        //        Board board = SolverBoard.SolverStringPositionToBoard(r.position);

        //        List<int> engineFull = SolverBoard.FullColumns(board);

        //        List<int> solverNull = new();
        //        for (int c = 0; c < r.columnScores.Length; c++)
        //        {
        //            if (r.columnScores[c] is null)
        //            {
        //                solverNull.Add(c);
        //            }
        //        }

        //        Assert.True(engineFull.SequenceEqual(solverNull), $"full columns within data and board do not match for record {r.position}");
        //    }
        //}

        //// Check that best columns match max score values
        //[Fact]
        //public void SolverBoard_BestColumnsMatchScores()
        //{
        //    foreach (Record r in Sample())
        //    {
        //        int max = int.MinValue;
        //        List<int> maxCols = new();

        //        for (int c = 0; c < r.columnScores.Length; c++)
        //        {
        //            int? score = r.columnScores[c];
        //            if (!score.HasValue)
        //            {
        //                continue;
        //            }

        //            if (score.Value > max)
        //            {
        //                max = score.Value;
        //                maxCols.Clear();
        //                maxCols.Add(c);
        //            }
        //            else if (score.Value == max)
        //            {
        //                maxCols.Add(c);
        //            }
        //        }

        //        Assert.True(maxCols.SequenceEqual(r.bestColumns), $"best column does not represent the best score for record {r.position}");
        //        Assert.Equal(max, r.value);
        //    }
        //}

        //// Check that best columns are playable in board
        //[Fact]
        //public void SolverBoard_BestColumnsLegalMoves()
        //{
        //    foreach (Record r in Sample())
        //    {
        //        Board board = SolverBoard.SolverStringPositionToBoard(r.position);
        //        foreach (int c in r.bestColumns)
        //        {
        //            Assert.True(board.IsValidMove(c), $"Best column {c} is not playable on board for record {r.position}");
        //        }
        //    }
        //}
    }
}