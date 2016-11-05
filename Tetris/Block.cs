using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tetris
{
    /// <summary>
    /// A block (tetromino) that falls down the grid
    /// </summary>
    class Block
    {
        /// <summary>
        /// The color of the block
        /// </summary>
        public Color color;

        /// <summary>
        /// The position of squares and blanks within the block.
        /// squares[row, col]. Col = 0, Row = 1 for GetLength()
        /// Not consistent with the board, but it makes the visualisations for
        /// the starting positions clearer.
        /// </summary>
        public Boolean[,] squares;

        /// <summary>
        /// The x coordinate of the block
        /// </summary>
        public int x;

        /// <summary>
        /// The y coordinate of the block
        /// </summary>
        public int y;

        /// <summary>
        /// A block (tetromino) that falls down the grid
        /// </summary>
        /// <param name="positionSpawner">The type of the block</param>
        public Block(BlockType blockType)
        {
            squares = blockType.shape;
            color = blockType.color;
        }

        /// <summary>
        /// Used to clone a block. Doesn't copy <code>squares</code>.
        /// </summary>
        /// <param name="original"></param>
        private Block(Block original)
        {
            //this.squares = (bool[,])original.squares.Clone();
            this.color = original.color;
            this.x = original.x;
            this.y = original.y;
        }

        /// <summary>
        /// Converts the coordinates of a square within the block to board-coodinate space
        /// </summary>
        /// <returns></returns>
        public Coordinate toBoardCoordinates(Coordinate coord)
        {
            coord.x += x;
            coord.y += y;

            return coord;
        }

        /// <summary>
        /// Returns the block rotated clockwise
        /// </summary>
        public Block RotatedClockwise()
        {
            return RotatedAntiClockwise().RotatedAntiClockwise().RotatedAntiClockwise();
        }

        /// <summary>
        /// Returns the block rotated anti-clockwise
        /// </summary>
        public Block RotatedAntiClockwise()
        {
            Block copy = new Block(this);

            // would be quicker to use matrices, but thinking is hard ;P
            copy.squares = new Boolean[squares.GetLength(0), squares.GetLength(1)];

            // works for squares of size 4x4, so hopefully also works for bigger ones
            for (int col = 0; col < squares.GetLength(0); col++)
                for (int row = 0; row < squares.GetLength(1); row++)
                    copy.squares[squares.GetLength(1) - 1 - row, col] = squares[col, row];

            return copy;
        }
    }
}
