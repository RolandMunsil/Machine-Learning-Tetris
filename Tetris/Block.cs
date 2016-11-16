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
        /// The coordinates of the squares of the block relative to topLeft
        /// </summary>
        public Coordinate[] squareCoords;

        public int boundingSquareSize;

        public Coordinate topLeft;

        /// <summary>
        /// A block (tetromino) that falls down the grid
        /// </summary>
        /// <param name="positionSpawner">The type of the block</param>
        public Block(BlockType blockType)
        {
            squareCoords = new Coordinate[blockType.squareCoords.Length];
            Array.Copy(blockType.squareCoords, squareCoords, blockType.squareCoords.Length);
            boundingSquareSize = blockType.boundingSquareSize;
            color = blockType.color;
            topLeft = new Coordinate(-1, -1);
        }

        /// <summary>
        /// Used to clone a block. Doesn't copy <code>squares</code>.
        /// </summary>
        /// <param name="original"></param>
        private Block(Block original)
        {
            //this.squares = (bool[,])original.squares.Clone();
            this.boundingSquareSize = original.boundingSquareSize;
            this.color = original.color;
            this.topLeft = new Coordinate(original.topLeft.row, original.topLeft.col);
        }

        /// <summary>
        /// Converts the coordinates of a square within the block to board-coodinate space
        /// </summary>
        /// <returns></returns>
        public Coordinate toBoardCoordinates(Coordinate coord)
        {
            return new Coordinate(coord.row + topLeft.row, coord.col + topLeft.col);
        }

        /// <summary>
        /// Returns the block rotated clockwise
        /// </summary>
        public Block RotatedClockwise()
        {
            Block copy = new Block(this);
            copy.squareCoords = new Coordinate[squareCoords.Length];

            for (int i = 0; i < squareCoords.Length; i++)
            {
                Coordinate orig = squareCoords[i];
                copy.squareCoords[i] = new Coordinate(orig.col, (boundingSquareSize - 1) - orig.row);
            }

            return copy;
        }
    }
}
