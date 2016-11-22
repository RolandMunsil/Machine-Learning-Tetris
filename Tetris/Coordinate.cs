using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris
{
    /// <summary>
    /// A coordinate in the form (row, col)
    /// </summary>
    class Coordinate
    {
        public int row;
        public int col;

        /// <summary>
        /// Create a new coordinate in the form (row, col)
        /// </summary>
        public Coordinate(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public Coordinate Clone()
        {
            return new Coordinate(this.row, this.col);
        }
    }
}
