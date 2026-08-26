namespace ConnectFour.Core
{
    public class BoardCopy
    {
        private readonly Disc[,] cells = new Disc[Board.Rows, Board.Columns]; // the board grid to copy
        private readonly int[] heights = new int[Board.Columns]; // array of heights to quickly determine top available row

        // Copy the current board state - loop each cell and fill it respectively
        public BoardCopy(Board board)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                // track how much the column is full
                int filled = 0;
                for (int r = 0; r < Board.Rows; r++)
                {
                    Disc d = board.CellAt(r, c);
                    this.cells[r, c] = d;
                    if (d != Disc.Empty) filled++;
                }
                // fill heights with each colum's height
                this.heights[c] = filled;
            }
        }

        public Disc CellAt(int row, int col)
        {
            return this.cells[row, col];
        }

        // Check if a column can be played using heights
        public bool CanPlay(int col)
        {
            return this.heights[col] < Board.Rows;
        }

        // Check if board is full - can any column be played?
        public bool IsFull()
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                if (CanPlay(c)) return false;
            }
            return true;
        }

        // Drop a disc into a column using the heights array for quickly finding
        // next avaiable row. 
        public int Drop(int col, Disc disc)
        {
            // Find top cell available
            int row = Board.Rows - 1 - this.heights[col];
            // Add disc
            this.cells[row, col] = disc;
            // Update heights
            this.heights[col]++;
            // Returns the row value just filled to get used for checking for a win
            return row;
        }

        // Undo the last row filled in a particular column. Used by the minimax search
        public void Undo(int col)
        {
            // Amend heights
            this.heights[col]--;
            // Find the row value to be undone
            int row = Board.Rows - 1 - this.heights[col];
            // Change cell to empty
            this.cells[row, col] = Disc.Empty;
        }

        // Return the number of adjacent cells matching the disc colur in a
        // particular direction (set by rowDirection, colDirection) starting from 
        // a particular cell (set by row, col)
        private int ScanCells(int row, int col, int rowDirection, int colDirection, Disc disc)
        {
            // Set a counter
            int count = 0;
            // set up row of current cell to scan
            int r = row + rowDirection;
            // set up column of current cell to scan
            int c = col + colDirection;
            // while the cell to scan remains in the grid boundaries and matches disc colur
            while (r >= 0 && r < Board.Rows && c >= 0 && c < Board.Columns
                   && this.cells[r, c] == disc)
            {
                // add one to the counter
                count++;
                // move the row index to the next cell
                r += rowDirection;
                // move the column index to the next cell
                c += colDirection;
            }
            return count;
        }

        // Tally up how many matching discs for the current disc were found
        private int CheckForLine(int row, int col, int rowDirection, int colDirection, Disc disc)
        {
            // 1 for the cell just played, count for one direction and count for the direct opposite direction
            return 1 + ScanCells(row, col, rowDirection, colDirection, disc)
                     + ScanCells(row, col, -rowDirection, -colDirection, disc);
        }

        // Check for a winning move on latest move (not the whole board)
        public bool IsWinningMove(int row, int col, Disc disc)
        {
            return CheckForLine(row, col, 0, 1, disc) >= 4 // horizontal
                || CheckForLine(row, col, 1, 0, disc) >= 4 // vertical
                || CheckForLine(row, col, 1, 1, disc) >= 4 // top left to bottom right (\)
                || CheckForLine(row, col, 1, -1, disc) >= 4; // bottom left to top right (/)
        }

    }
}
