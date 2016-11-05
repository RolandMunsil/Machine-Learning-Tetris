using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tetris
{
    /// <summary>
    /// The game board
    /// </summary>
    class Board
    {
        /// <summary>
        /// The game board for a game of Tetris
        /// </summary>
        /// <param name="noOfRows">The number of visible rows on the board</param>
        /// <param name="noOfColumns">The number of columns on the board</param>
        public Board(int noOfRows, int noOfColumns, String blockSet)
        {
            board = new int[noOfColumns, noOfRows + numHiddenRows];
            blockSpawner = new BlockSpawner(blockSet);

            // initialise the board by setting each cell as the default color
            for (int col = 0; col < noOfColumns; col++)
                for (int row = 0; row < noOfRows + numHiddenRows; row++)
                    board[col, row] = boardColor;

            // initialise variables based on the parameters
            numColumns = noOfColumns;
            numVisibleRows = noOfRows;
            totalNumRows = noOfRows + numHiddenRows;

            SpawnBlock();
        }

        #region variables

        /// <summary>
        /// A block spawner to specify the blocks to spawn
        /// </summary>
        BlockSpawner blockSpawner;

        /// <summary>
        /// The number of rows that have been destroyed
        /// </summary>
        public int rowsDestroyed = 0;

        /// <summary>
        /// The score that has been obtained
        /// </summary>
        public int score = 0;

        /// <summary>
        /// The default color of the board when there are no blocks there
        /// </summary>
        int boardColor = Color.PeachPuff.ToArgb();

        /// <summary>
        /// The board that is being played on.
        /// board[col, row]. (0, 0) is the upper left.
        /// </summary>
        public int[,] board;

        /// <summary>
        /// The block that is currently being played
        /// </summary>
        public Block currentBlock;

        /// <summary>
        /// The number of visible columns on the board
        /// </summary>
        int numColumns;

        /// <summary>
        /// The number of rows that are hidden above the top of the grid
        /// </summary>
        public readonly int numHiddenRows = 20;

        /// <summary>
        /// The number of rows on the board
        /// </summary>
        int numVisibleRows;

        /// <summary>
        /// The total number of rows on the board
        /// </summary>
        int totalNumRows;

        #endregion variables

        /// <summary>
        /// Ticks the board forward one move
        /// </summary>
        public void Tick()
        {
            if (!CanLowerBlock())
            {
                LockBlock();
                SpawnBlock();
            }

            TryLowerBlock();
            DestroyFullRows();
        }

        #region board

        /// <summary>
        /// Creates a new block to play with
        /// </summary>
        private void SpawnBlock()
        {
            // spawn a new block
            currentBlock = blockSpawner.Next();
            currentBlock.y = numHiddenRows - 2;
            currentBlock.x = (numColumns - currentBlock.squares.GetLength(1)) / 2;
        }

        /// <summary>
        /// Locks the current block into position on the board
        /// </summary>
        private void LockBlock()
        {
            // loop through each of the squares within the current block
            for (int row = 0; row < currentBlock.squares.GetLength(0); row++)
            {
                for (int col = 0; col < currentBlock.squares.GetLength(1); col++)
                {
                    // if there's something there
                    if (currentBlock.squares[row, col])
                    {
                        Coordinate coord = currentBlock.toBoardCoordinates(new Coordinate(col, row));
                        // lock it into position on the board
                        board[coord.x, coord.y] = currentBlock.color.ToArgb();
                    }
                }
            }
        }

        #region gameEvents

        /// <summary>
        /// Checks each of the rows and removes it if it's full, starting at the top and moving down.
        /// </summary>
        private void DestroyFullRows()
        {
            int rowsDestroyedStart = rowsDestroyed;

            for (int row = numHiddenRows; row < totalNumRows; row++)
            {
                if (RowIsFull(row))
                    DestroyRow(row);
            }

            // give bonus points for clearing multiple rows at a time
            score += (rowsDestroyed - rowsDestroyedStart) * (rowsDestroyed - rowsDestroyedStart);
        }

        /// <summary>
        /// Checks to see whether the specified row is full and should be removed
        /// </summary>
        /// <param name="rowToCheck">The row to check</param>
        /// <returns>Whether the specified row is full</returns>
        private Boolean RowIsFull(int row)
        {
            for (int col = 0; col < numColumns; col++)
                if (board[col, row] == boardColor)
                    return false;

            return true;
        }

        /// <summary>
        /// Removes a row from the game board and drops the remaining squares down from above
        /// </summary>
        /// <param name="row">The row to remove</param>
        private void DestroyRow(int rowToRemove)
        {
            if (rowToRemove == 0)
                throw new IndexOutOfRangeException();

            // start on the specified row and move up
            for (int row = rowToRemove; row > 0; row--)
            {
                // passing through each column
                for (int col = 0; col < numColumns; col++)
                {
                    // and overwriting the current position with the one above
                    board[col, row] = board[col, row - 1];
                }
            }

            rowsDestroyed++;
        }

        public bool TopRowHasSquare()
        {
            int row = numVisibleRows - 1;
            for (int col = 0; col < numColumns; col++)
            {
                if (board[col, row] != boardColor)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion gameEvents

        #region Block Movement
        /// <summary>
        /// Rotates the block 90 degrees clockwise if possible
        /// </summary>
        public void TryRotateBlock()
        {
            Block rotated = currentBlock.RotatedClockwise();
            if (CanBeHere(rotated))
            {
                currentBlock = rotated;
            }
        }

        /// <summary>
        /// Lowers the current block down one row if possible
        /// </summary>
        /// <returns>Whether the block could be lowered</returns>
        public void TryLowerBlock()
        {
            currentBlock.y++;

            if (!CanBeHere(currentBlock))
                currentBlock.y--;
        }

        public bool CanLowerBlock()
        {
            currentBlock.y++;
            bool worked = CanBeHere(currentBlock);
            currentBlock.y--;
            return worked;
        }

        /// <summary>
        /// Moves the block left one column if possible
        /// </summary>
        public void TryMoveBlockLeft()
        {
            currentBlock.x--;

            if (!CanBeHere(currentBlock))
                currentBlock.x++;
        }

        /// <summary>
        /// Moves the block right one column if possible
        /// </summary>
        public void TryMoveBlockRight()
        {
            currentBlock.x++;

            if (!CanBeHere(currentBlock))
                currentBlock.x--;
        }
        #endregion blockMovement

        /// <summary>
        /// Checks to see whether the block is allowed to be in the specified position
        /// </summary>
        /// <param name="block">The block to check</param>
        /// <returns>Whether the block is allowed to be there</returns>
        private Boolean CanBeHere(Block block)
        {
            // loop through each of the squares within the current block
            for (int col = 0; col < block.squares.GetLength(0); col++)
            {
                for (int row = 0; row < block.squares.GetLength(1); row++)
                {
                    // if there's something there
                    if (block.squares[row, col])
                    {
                        // check to see if there's something already here
                        Coordinate coord = block.toBoardCoordinates(new Coordinate(col, row));

                        if (coord.x >= numColumns || coord.x < 0 || 
                            coord.y >= totalNumRows || board[coord.x, coord.y] != boardColor)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion board
    }
}
