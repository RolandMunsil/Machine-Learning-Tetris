using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tetris
{
    [System.Diagnostics.DebuggerDisplay("{color.Name}")]
    /// <summary>
    /// Specifies a type of block (i.e. a shape and color)
    /// </summary>
    class BlockType
    {
        /// <summary>
        /// The coordinates of the squares of the block
        /// </summary>
        public Coordinate[] squareCoords;

        public int boundingSquareSize;

        /// <summary>
        /// The color of the block - regulations specify that different shapes are different colors
        /// </summary>
        public Color color;
    }
}
