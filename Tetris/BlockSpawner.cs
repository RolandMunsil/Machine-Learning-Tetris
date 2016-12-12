using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tetris
{
    class BlockSpawner
    {
        /// <summary>
        /// The sequence of blocks that should be taken from when selecting a new one
        /// </summary>
        private BlockType[] sequence;

        /// <summary>
        /// A random number generator
        /// </summary>
        private Random rand = new Random();

        /// <summary>
        /// Index of the next block to pick
        /// </summary>
        private int nextIndex = 0;

        public BlockSpawner(Random rand)
        {
            this.rand = rand;
            sequence = GenerateBlockTypeSequence();
        }

        /// <summary>
        /// Returns the type of the next block to play with
        /// </summary>
        public Block Next()
        {
            if(nextIndex >= sequence.Length)
            {
                nextIndex = 0;
                sequence = GenerateBlockTypeSequence();
            }
            return new Block(sequence[nextIndex++]);
        }

        /// <summary>
        /// Generate a shuffled list of block types
        /// </summary>
        private BlockType[] GenerateBlockTypeSequence()
        {
            BlockType[] sequence = GenerateDefaultBlockTypes();
            Shuffle(sequence);
            return sequence;
        }

        /// <summary>
        /// If no block types have been loaded from a file, just use the normal Tetris blocks
        /// </summary>
        private BlockType[] GenerateDefaultBlockTypes()
        {
            BlockType[] types = new BlockType[7];

            // straight across in a line
            types[0] = new BlockType();
            types[0].boundingSquareSize = 4;
            types[0].squareCoords = new[] {new Coordinate(2, 0),
                                           new Coordinate(2, 1),
                                           new Coordinate(2, 2),
                                           new Coordinate(2, 3)};
            types[0].color = Color.Cyan;

            // L with a spike on the left
            types[1] = new BlockType();
            types[1].boundingSquareSize = 3;
            types[1].squareCoords = new[] {new Coordinate(0, 0),
                                           new Coordinate(1, 0),
                                           new Coordinate(1, 1),
                                           new Coordinate(1, 2)};
            types[1].color = Color.Blue;

            // L with a spike on the right
            types[2] = new BlockType();
            types[2].boundingSquareSize = 3;
            types[2].squareCoords = new[] {new Coordinate(0, 2),
                                           new Coordinate(1, 0),
                                           new Coordinate(1, 1),
                                           new Coordinate(1, 2)};
            types[2].color = Color.Orange;

            // square
            types[3] = new BlockType();
            types[3].boundingSquareSize = 2;
            types[3].squareCoords = new[] {new Coordinate(0, 0),
                                           new Coordinate(1, 0),
                                           new Coordinate(0, 1),
                                           new Coordinate(1, 1)};
            types[3].color = Color.Yellow;

            // zig-zag up to the right
            types[4] = new BlockType();
            types[4].boundingSquareSize = 3;
            types[4].squareCoords = new[] {new Coordinate(2, 0),
                                           new Coordinate(2, 1),
                                           new Coordinate(1, 1),
                                           new Coordinate(1, 2)};
            types[4].color = Color.Red;

            // zig-zag up to the left
            types[5] = new BlockType();
            types[5].boundingSquareSize = 3;
            types[5].squareCoords = new[] {new Coordinate(1, 0),
                                           new Coordinate(1, 1),
                                           new Coordinate(2, 1),
                                           new Coordinate(2, 2)};

            types[5].color = Color.Green;

            // T shape
            types[6] = new BlockType();
            types[6].boundingSquareSize = 3;
            types[6].squareCoords = new[] {new Coordinate(1, 0),
                                           new Coordinate(1, 1),
                                           new Coordinate(1, 2),
                                           new Coordinate(0, 1)};
            types[6].color = Color.Purple;
            return types;
        }

        /// <summary>
        /// Shuffle an array of block types using Fisher-Yates
        /// </summary>
        /// <param name="bucket">The array that should be shuffled</param>
        private void Shuffle(BlockType[] array)
        {
            BlockType temp;
            for (int i = array.Length - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                temp = array[i];
                array[i] = array[n];
                array[n] = temp;
            }
        }
    }
}
