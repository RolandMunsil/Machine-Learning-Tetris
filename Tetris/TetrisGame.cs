using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tetris;

namespace Tetris
{
    public partial class TetrisGame : Form
    {
        /// <summary>
        /// Is a game currently being played?
        /// </summary>
        bool playing;

        /// <summary>
        /// Whether to update the UI when the board state changes
        /// </summary>
        bool showUIWhenPlaying;

        /// <summary>
        /// The board that the game is played on
        /// </summary>
        Board board;

        /// <summary>
        /// The number of visible columns on the board
        /// </summary>
        int numberOfColumns = 10;

        /// <summary>
        /// The number of visible rows on the board
        /// </summary>
        int numberOfRows = 20;

        /// <summary>
        /// The size in px of each side of the squares
        /// </summary>
        int squareDimensions = 20;

        /// <summary>
        /// The squares that are visible on the board
        /// </summary>
        Dictionary<string, Square> squares = new Dictionary<string, Square>();

        /// <summary>
        /// A game of Tetris implemented as a WinForms application
        /// </summary>
        public TetrisGame()
        {
            InitializeComponent();
            CreateSquares();
            blocksetList.DataSource = BlockLoader.names();
            showUIWhenPlaying = true;
        }      

        #region game

        /// <summary>
        /// Resets the game
        /// </summary>
        private void ResetGame()
        {
            bool boardPropertiesChanged = squareDimensions != (int)sqSizeSelect.Value ||
                                              numberOfRows != (int)boardRowsSelect.Value ||
                                           numberOfColumns != (int)boardColsSelect.Value;
            if (boardPropertiesChanged)
            {
                squareDimensions = (int)sqSizeSelect.Value;
                numberOfRows = (int)boardRowsSelect.Value;
                numberOfColumns = (int)boardColsSelect.Value;
                if (showUIWhenPlaying)
                    CreateSquares();
            }
            board = new Board(numberOfRows, numberOfColumns, blocksetList.Text);
            
            if (showUIWhenPlaying)
                tickTimer.Enabled = true;
            playing = true;
        }

        /// <summary>
        /// Moves the current block down and 
        /// </summary>
        private void tickTimer_Tick(object sender, EventArgs e)
        {
            board.Tick();
            UpdateBoard();
            rowsCleared.Text = board.rowsDestroyed.ToString();
            score.Text = board.score.ToString();
        }

        #endregion game

        #region GUI

        /// <summary>
        /// Calculates a string to use as the key for the squares hash
        /// </summary>
        private String SquaresKey(int row, int col)
        {
            return "R" + row.ToString() + "C" + col.ToString();
        }

        /// <summary>
        /// Creates the squares which make up the visible portion of the board
        /// </summary>
        private void CreateSquares()
        {
            foreach (Square square in squares.Values)
            {
                square.Dispose();
            }
            squares.Clear();

            for (int row = 0; row < numberOfRows; row++)
            {
                for (int col = 0; col < numberOfColumns; col++)
                {
                    Square square = new Square(row, col);
                    square.Width = squareDimensions;
                    square.Height = squareDimensions;
                    square.Parent = gameWindow;
                    square.Top = row * squareDimensions;
                    square.Left = col * squareDimensions;

                    squares.Add(SquaresKey(row, col), square);
                }
            }
        }

        /// <summary>
        /// Updates the game board, displaying where the squares are on the grid
        /// </summary>
        private void UpdateBoard()
        {
            // update the color of each of the squares on the board
            Square square;
            for (int row = 0; row < numberOfRows; row++)
            {
                for (int col = 0; col < numberOfColumns; col++)
                {
                    squares.TryGetValue(SquaresKey(row, col), out square);
                    square.color = board.board[col, row + board.hiddenRows];
                }
            }

            // then display the current block
            Block block = board.currentBlock; // cache the block to make the code read a bit better
            for (int row = 0; row < block.squares.GetLength(0); row++)
            {
                for (int col = 0; col < block.squares.GetLength(1); col++)
                {
                    Coordinate coord = new Coordinate(col, row);
                    coord = block.toBoardCoordinates(coord);
                    if (block.squares[row, col] && coord.x >= 0 && coord.x < numberOfColumns
                            && coord.y >= board.hiddenRows && coord.y < numberOfRows + board.hiddenRows)
                    {
                        squares.TryGetValue(SquaresKey(coord.y - board.hiddenRows, coord.x), out square);
                        square.color = block.color.ToArgb();
                    }
                }
            }
        }

        #endregion GUI

        #region Input
        /// <summary>
        /// Create a new game when the 'New Game' button is clicked
        /// </summary>
        private void newGameButton_Click(object sender, EventArgs e)
        {
            ResetGame();
        }

        /// <summary>
        /// Listen for input from the keyboard and respond accordingly
        /// </summary>
        private void TetrisGame_KeyPress(object sender, KeyPressEventArgs e)
        {
            textBox1.Text = e.KeyChar.ToString();
            if (playing)
            {
                switch(e.KeyChar)
                {
                    case 's':
                        board.TryLowerBlock();
                        break;
                    case 'a':
                        board.TryMoveBlockLeft();
                        break;
                    case 'd':
                        board.TryMoveBlockRight();
                        break;
                    case 'w':
                        board.TryRotateBlock();
                        break;
                }
                UpdateBoard();
            }
        }
        #endregion

        private void learnButton_Click(object sender, EventArgs e)
        {
            //Just testing that we can quickly play a game
            showUIWhenPlaying = false;
            ResetGame();

            Random rand = new Random();

            for(int i = 0; !board.topRowHasSquare(); i++) //Losing situation
            {
                //Thread.Sleep(30);
                if (rand.NextDouble() > 0.5)
                {
                    board.TryMoveBlockLeft();
                }
                else
                {
                    board.TryMoveBlockRight();
                }

                if (i % 5 == 4)
                {
                    //Moves block down, adds new block if necessary, removes any filled rows, updates score
                    board.Tick();
                    //Update squares then draw bloack
                    //updateBoard();
                    if (showUIWhenPlaying)
                    {
                        rowsCleared.Text = board.rowsDestroyed.ToString();
                        score.Text = board.score.ToString();
                    }
                }

                if (showUIWhenPlaying)
                {
                    UpdateBoard();
                    //This is a hack, in the future start a separate thread
                    Application.DoEvents();
                }
            }
            tickTimer.Enabled = false;

            if (!showUIWhenPlaying)
            {
                //We need to show the final state.
                CreateSquares();
                UpdateBoard();
            }
        }
    }
}
