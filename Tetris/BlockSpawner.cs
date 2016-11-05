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

        /// <summary>
        /// Block shapes that have been loaded from a file
        /// </summary>
        private List<String[]> loadedBlockTypes = null;

        public BlockSpawner()
        {
            sequence = GenerateBlockTypeSequence();
        }

        /// <summary>
        /// Create a block type spawner where the block positions are loaded from a file
        /// </summary>
        /// <param name="name">The name of the file to load</param>
        public BlockSpawner(String name)
        {
            loadedBlockTypes = BlockLoader.load(name);

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
            if (loadedBlockTypes == null || loadedBlockTypes.Count == 0)
            {
                BlockType[] sequence = GenerateDefaultBlockTypes();
                Shuffle(sequence);
                return sequence;
            }
            else
            {
                BlockType[] sequence = new BlockType[7];

                int count = 0;

                foreach (String[] block in loadedBlockTypes)
                {
                    sequence[count] = new BlockType();
                    sequence[count].shape = new Boolean[block.Length, block.Length];
                    // load the positions
                    for (var row = 0; row < block.Length; row++)
                    {
                        char[] rowArr = block[row].ToCharArray();
                        for (var col = 0; col < block[row].Length; col++)
                        {
                            sequence[count].shape[row, col] = rowArr[col] == BlockLoader.solidIndicator;
                        }
                    }
                    // give it a random color. Pretty ^_^
                    KnownColor[] names = (KnownColor[]) Enum.GetValues(typeof(KnownColor));
                    KnownColor randomColorName = names[rand.Next(names.Length)];
                    sequence[count].color = Color.FromKnownColor(randomColorName);
                    
                    count++;
                }
                Shuffle(sequence);
                return sequence;
            }
        }

        /// <summary>
        /// If no block types have been loaded from a file, just use the normal Tetris blocks
        /// </summary>
        private BlockType[] GenerateDefaultBlockTypes()
        {
            BlockType[] types = new BlockType[7];
            // straight across in a line
            types[0] = new BlockType();
            types[0].shape = new Boolean[,]
                                        {{false, false, false, false},
                                        {false, false, false, false},
                                        {true, true, true, true},
                                        {false, false, false, false}};
            types[0].color = Color.Cyan;
            // L with a spike on the left
            types[1] = new BlockType();
            types[1].shape = new Boolean[,]
                                        {{true, false, false},
                                         {true, true, true},
                                         {false, false, false}};
            types[1].color = Color.Blue;
            // L with a spike on the right
            types[2] = new BlockType();
            types[2].shape = new Boolean[,]
                                        {{false, false, true},
                                         {true, true, true},
                                         {false, false, false}};
            types[2].color = Color.Orange;
            // square
            types[3] = new BlockType();
            types[3].shape = new Boolean[,]
                                        {{true, true},
                                        {true, true}};
            types[3].color = Color.Yellow;
            // zig-zag up to the right
            types[4] = new BlockType();
            types[4].shape = new Boolean[,]
                                        {{false, false, false},
                                        {false, true, true},
                                        {true, true, false}};
            types[4].color = Color.Red;
            // zig-zag up to the left
            types[5] = new BlockType();
            types[5].shape = new Boolean[,]
                                        {{false, false, false},
                                        {true, true, false},
                                        {false, true, true}};
            types[5].color = Color.Green;
            // T shape
            types[6] = new BlockType();
            types[6].shape = new Boolean[,]
                                        {{false, true, false},
                                        {true, true, true},
                                        {false, false, false}};
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
