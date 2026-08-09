using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectFour.Core.Tests
{
    public class BoardTests
    {
        // A brand new board should have every cell set to Empty
        [Fact]
        public void Board_EmptyCellsAtStart()
        {
            Board board = new Board();

            // loop through each cell - check they are empty
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Columns; c++)
                {
                    Assert.Equal(Disc.Empty, board.CellAt(r, c));
                }
            }
        }

        // A brand new board is not full
        [Fact]
        public void Board_IsNotFullAtStart()
        {
            Board board = new Board();

            Assert.False(board.IsFull());
        }

        // Check valid moves without bounds of the board
        [Theory]
        [InlineData(0, true)]
        [InlineData(3, true)]
        [InlineData(6, true)]
        [InlineData(-1, false)]
        [InlineData(7, false)]
        [InlineData(100, false)]
        public void Board_ChecksBounds(int column, bool expected)
        {
            Board board = new Board();

            Assert.Equal(expected, board.IsValidMove(column));
        }

        // Method to help fill board quicker - drop the same disc into a column 'count' times.
        private static void Fill(Board board, int column, Disc disc, int count)
        {
            for (int i = 0; i < count; i++)
            {
                board.DropDisc(column, disc);
            }
        }

        // Once a column is full, it is no longer a valid move
        [Fact]
        public void Board_ValidMoveFullColumn()
        {
            Board board = new Board();

            // fill column 0 completely (Rows discs)
            Fill(board, 0, Disc.Red, Board.Rows);

            Assert.False(board.IsValidMove(0));
        }

        // A valid drop returns true
        [Fact]
        public void Board_DropDiscValidColumn()
        {
            Board board = new Board();

            Assert.True(board.DropDisc(3, Disc.Red));
        }

        // Trying to drop disc in a full column returns false
        [Fact]
        public void Board_DropDiscInvalidColumn()
        {
            Board board = new Board();

            Fill(board, 0, Disc.Red, Board.Rows);

            Assert.False(board.DropDisc(0, Disc.Yellow));
        }

        // Dropping into an out-of-bounds column returns false
        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        [InlineData(99)]
        public void Board_DropDiscColumnBounds(int column)
        {
            Board board = new Board();

            Assert.False(board.DropDisc(column, Disc.Red));
        }

        // Test that disc drops to bottom row
        [Fact]
        public void Board_FirstDiscLandsAtBottom()
        {
            Board board = new Board();

            board.DropDisc(2, Disc.Red);

            Assert.Equal(Disc.Red, board.CellAt(Board.Rows - 1, 2));
        }

        // Test that a second disc stacks directly on top of the first
        [Fact]
        public void Board_SecondDiscStacksOnTop()
        {
            Board board = new Board();

            board.DropDisc(2, Disc.Red);      // goes to bottom row
            board.DropDisc(2, Disc.Yellow);   // goes to second row

            Assert.Equal(Disc.Red, board.CellAt(Board.Rows - 1, 2));
            Assert.Equal(Disc.Yellow, board.CellAt(Board.Rows - 2, 2));
        }

        // Discs land in the right column for a range of columns
        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(6)]
        public void Board_PlacesInCorrectColumn(int column)
        {
            Board board = new Board();

            board.DropDisc(column, Disc.Red);

            Assert.Equal(Disc.Red, board.CellAt(Board.Rows - 1, column));
        }

        // Check for horizontal win
        [Fact]
        public void Board_CheckHorizontalWinTrue()
        {
            Board board = new Board();

            board.DropDisc(0, Disc.Red);
            board.DropDisc(1, Disc.Red);
            board.DropDisc(2, Disc.Red);
            board.DropDisc(3, Disc.Red);

            Assert.True(board.CheckWin(Disc.Red));
        }

        // Check for vertical win
        [Fact]
        public void Board_CheckVerticalWinTrue()
        {
            Board board = new Board();

            Fill(board, 0, Disc.Yellow, 4);

            Assert.True(board.CheckWin(Disc.Yellow));
        }

        // Check for down-right diagonal
        [Fact]
        public void Board_CheckDownRightDiagonalWinTrue()
        {
            Board board = new Board();

            // Column 0: three Yellow then Red
            Fill(board, 0, Disc.Yellow, 3);
            board.DropDisc(0, Disc.Red);
            // Column 1: two Yellow then Red
            Fill(board, 1, Disc.Yellow, 2);
            board.DropDisc(1, Disc.Red);
            // Column 2: one Yellow then Red
            board.DropDisc(2, Disc.Yellow);
            board.DropDisc(2, Disc.Red);
            // column 3: one Red
            board.DropDisc(3, Disc.Red);

            Assert.True(board.CheckWin(Disc.Red));
        }

        // Check for up-right diagonal
        [Fact]
        public void Board_CheckUpRightDiagonalWinTrue()
        {
            Board board = new Board();

            // Column 0: one Red
            board.DropDisc(0, Disc.Red);
            // Column 1: one Yellow then Red
            board.DropDisc(1, Disc.Yellow);
            board.DropDisc(1, Disc.Red);
            // Column 2: two Yellow then Red
            Fill(board, 2, Disc.Yellow, 2);
            board.DropDisc(2, Disc.Red);
            // Column 3: three Yellow then Red
            Fill(board, 3, Disc.Yellow, 3);
            board.DropDisc(3, Disc.Red);

            Assert.True(board.CheckWin(Disc.Red));
        }

        // Check an empty board has no winner
        [Fact]
        public void Board_CheckWinEmptyBoard()
        {
            Board board = new Board();

            Assert.False(board.CheckWin(Disc.Red));
            Assert.False(board.CheckWin(Disc.Yellow));
        }

        // Check that a three in a row is not a win
        [Fact]
        public void Board_CheckWinThreeInARow()
        {
            Board board = new Board();

            board.DropDisc(0, Disc.Red);
            board.DropDisc(1, Disc.Red);
            board.DropDisc(2, Disc.Red);

            Assert.False(board.CheckWin(Disc.Red));
        }

        // Test that a win for one colour is not reported for the other colour
        [Fact]
        public void Board_CheckWinCorrectColour()
        {
            Board board = new Board();

            board.DropDisc(0, Disc.Red);
            board.DropDisc(1, Disc.Red);
            board.DropDisc(2, Disc.Red);
            board.DropDisc(3, Disc.Red);

            Assert.True(board.CheckWin(Disc.Red));
            Assert.False(board.CheckWin(Disc.Yellow));
        }

        // Test full board returns true for IsFull
        [Fact]
        public void Board_IsFullOnFullBoard()
        {
            Board board = new Board();

            for (int c = 0; c < Board.Columns; c++)
            {
                Fill(board, c, Disc.Red, Board.Rows);
            }

            Assert.True(board.IsFull());
        }

        // Check a board with one empty column is not full
        [Fact]
        public void Board_IsFullOneColumnOpen()
        {
            Board board = new Board();

            // fill every column except the last
            for (int c = 0; c < Board.Columns - 1; c++)
            {
                Fill(board, c, Disc.Red, Board.Rows);
            }

            Assert.False(board.IsFull());
        }
    }
}