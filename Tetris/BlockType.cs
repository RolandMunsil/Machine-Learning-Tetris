using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tetris
{
    /// <summary>
    /// Specifies a type of block (i.e. a shape and color)
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{color.Name}")]
    class BlockType
    {
        /// <summary>
        /// The coordinates of the squares of the block
        /// </summary>
        public Coordinate[] squareCoords;

        /// <summary>
        /// The width and height of the bounding box of the block.
        /// </summary>
        public int boundingSquareSize;

        /// <summary>
        /// The color of the block
        /// </summary>
        public Color color;
    }
}
