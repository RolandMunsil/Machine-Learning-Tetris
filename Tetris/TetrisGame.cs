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
using Tetris.NEAT;
using Microsoft.VisualBasic.PowerPacks;
using System.Diagnostics;

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
        Square[,] squares;

        ShapeContainer lineContainer;

        /// <summary>
        /// A game of Tetris implemented as a WinForms application
        /// </summary>
        public TetrisGame()
        {
            InitializeComponent();
            CreateSquares();
            lineContainer = new ShapeContainer();
            lineContainer.Parent = gameWindow;
            lineContainer.BringToFront();
            //blocksetList.DataSource = BlockLoader.names();
            showUIWhenPlaying = true;
        }      

        #region game

        /// <summary>
        /// Resets the game
        /// </summary>
        private void ResetGame()
        {
            if (showUIWhenPlaying)
                CreateSquares();
            board = new Board(numberOfRows, numberOfColumns, new Random());
            
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
        /// Creates the squares which make up the visible portion of the board
        /// </summary>
        private void CreateSquares()
        {
            if (squares != null)
            {
                foreach (Square square in squares)
                {
                    square.Dispose();
                }
            }
            squares = new Square[numberOfRows, numberOfColumns];

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

                    squares[row, col] = square;
                }
            }
        }

        /// <summary>
        /// Updates the game board, displaying where the squares are on the grid
        /// </summary>
        private void UpdateBoard()
        {
            // update the color of each of the squares on the board
            for (int row = 0; row < numberOfRows; row++)
            {
                for (int col = 0; col < numberOfColumns; col++)
                {
                    if (board[row, col] == Board.LOCKED_SQUARES)
                    {
                        squares[row, col].Color = board.lockedBlocksColors[row, col];
                    }
                    else
                    {
                        squares[row, col].Color = Color.PeachPuff;
                    }
                }
            }

            // then display the current block
            Block block = board.currentBlock; // cache the block to make the code read a bit better
            foreach (Coordinate squareCoord in board.currentBlock.squareCoords)
            {
                Coordinate coord = block.toBoardCoordinates(squareCoord);
                if (coord.col >= 0 && coord.col < numberOfColumns
                        && coord.row >= 0 && coord.row < numberOfRows)
                {
                    squares[coord.row, coord.col].Color = block.color;
                }
            }

            lineContainer.Refresh();
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
            //textBox1.Text = e.KeyChar.ToString();
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
            new Thread(Learn).Start();

            //neat.MakeNextGeneration();
            /*
            //Just testing that we can quickly play a game
            showUIWhenPlaying = false;
            ResetGame();

            Random rand = new Random();

            for(int i = 0; !board.hasLost; i++) //Losing situation
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
                    //http://www.yoda.arachsys.com/csharp/threads/winforms.shtml
                    Application.DoEvents();
                }
            }
            tickTimer.Enabled = false;

            if (!showUIWhenPlaying)
            {
                //We need to show the final state.
                UpdateBoard();
                rowsCleared.Text = board.rowsDestroyed.ToString();
                score.Text = board.score.ToString();
            }
            */
        }

        private void Learn()
        {
            for (int i = 0; i < 100; i++)
            {
                int seed = Environment.TickCount;
                NEAT.NEAT xorNeat = new NEAT.NEAT();
                xorNeat.MakeGenZeroXOR();
                xorNeat.EvalGenerationXOR();

                var originalFitnesses = xorNeat.allSpecies.SelectMany(s => s.members).Select(o => o.fitness);
                //var adjFitnesses = xorNeat.allSpecies.SelectMany(s => s.members).Select(o => o.fitness);
                AddLineToTextBox($"Gen {xorNeat.currentGeneration}: avg orig fitness {originalFitnesses.Average()}, max {originalFitnesses.Max()}");
                //AddLineToTextBox($"Gen {xorNeat.currentGeneration}: avg adj  fitness {adjFitnesses.Average()}, max {adjFitnesses.Max()}");
                while (true)
                {
                    xorNeat.MakeNextGeneration();
                    xorNeat.EvalGenerationXOR();

                    originalFitnesses = xorNeat.allSpecies.SelectMany(s => s.members).Select(o => o.fitness);
                    //adjFitnesses = xorNeat.allSpecies.SelectMany(s => s.members).Select(o => o.fitness);
                    AddLineToTextBox($"Gen {xorNeat.currentGeneration}: avg orig fitness {originalFitnesses.Average()}, max {originalFitnesses.Max()}");
                    //AddLineToTextBox($"Gen {xorNeat.currentGeneration}: avg adj  fitness {adjFitnesses.Average()}, max {adjFitnesses.Max()}");
                    AddLineToTextBox($"    Mutated organisms: {xorNeat.genCreationInfo.organismsCreatedFromMutation + xorNeat.genCreationInfo.organismsCreatedFromMatingAndMutation}");
                    AddLineToTextBox($"    Node mutations: {xorNeat.genCreationInfo.nodeMutationsDone}");
                    double percentConnectionMutationsSucceeded = 100 * xorNeat.genCreationInfo.connectionMutationsDone / (double)xorNeat.genCreationInfo.connectionMutationsAttempted;
                    AddLineToTextBox($"    Connection mutations: {xorNeat.genCreationInfo.connectionMutationsDone} ({xorNeat.genCreationInfo.connectionMutationsAttempted} attempted)");
                    AddLineToTextBox($"    Species created: {xorNeat.genCreationInfo.newSpeciesCreated.Count}");
                    AddLineToTextBox($"    Species removed: {xorNeat.genCreationInfo.speciesRemovedBecauseStagnation.Count}+{xorNeat.genCreationInfo.speciesRemovedBecauseNoMembers.Count}");
                    String speciesStr = "[" + String.Join("][", xorNeat.allSpecies.Select(s => s.members.Count)) + "]";
                    AddLineToTextBox($"    {xorNeat.allSpecies.Count} species: {speciesStr}");
                    Thread.Sleep(10);
                    bool solutionFound = false;
                    foreach (Organism o in xorNeat.allSpecies.SelectMany(s => s.members))
                    {
                        solutionFound =
                            o.neuralNet.FeedForward(new[] { 1.0, 1.0, 1.0 })[0] < 0.5 &&
                            o.neuralNet.FeedForward(new[] { 1.0, 0.0, 1.0 })[0] >= 0.5 &&
                            o.neuralNet.FeedForward(new[] { 0.0, 1.0, 1.0 })[0] >= 0.5 &&
                            o.neuralNet.FeedForward(new[] { 0.0, 0.0, 1.0 })[0] < 0.5;
                        if (solutionFound)
                        {
                            AddLineToTextBox($"!!! Found solution @ gen {xorNeat.currentGeneration} !!!");
                            Thread.Sleep(3000);
                            //Debugger.Break();
                            break;
                        }
                    }
                    if (solutionFound)
                        break;
                }
            }

            return;

            NEAT.NEAT neat = new NEAT.NEAT();
            neat.MakeGenerationZero();
            neat.EvaluateGeneration();

            for (int i = 0; i < 40; i++)
            {
                neat.MakeNextGeneration();
                textBox1.Invoke(new Action(() => textBox1.AppendText($"Generation {neat.currentGeneration}\r\n")));
                neat.EvaluateGeneration();
            }
            SetText(genLabel, $"Generation {neat.currentGeneration}");
            foreach (Species species in neat.allSpecies.OrderByDescending(s => s.AverageFitness))
            {
                SetText(speciesLabel, $"Species {species.speciesNumber}/{neat.allSpecies.Count}");
                foreach (Organism organism in species.members)
                {
                    SetText(organismLabel, $"Organism {species.members.IndexOf(organism) + 1}/{species.members.Count}");
                    SetText(fitnessLabel, $"Original fitness: {organism.fitness}");

                    this.Invoke(new Action<Genome>(DrawNeuralNet), organism.genome);

                    board = new Board(20, 10, new Random(0));
                    while (!board.hasLost)
                    {
                        for (int i = 0; i < neat.movesAllowedBetweenTicks; i++)
                        {
                            //Construct inputs
                            neat.NetworkStep(organism.neuralNet, board);
                            this.Invoke(new Action(UpdateBoard));
                            Thread.Sleep(10);
                        }
                        board.Tick();
                        this.Invoke(new Action(UpdateBoard));
                        Thread.Sleep(10);
                    }
                }
            }
        }

        private void SetText(Label label, String text)
        {
            label.Invoke(new Action(() => label.Text = text));
        }

        private void AddLineToTextBox(String str)
        {
            textBox1.Invoke(new Action(() => textBox1.AppendText(str + "\r\n")));
        }

        private void DrawNeuralNet(Genome genome)
        {
            while (lineContainer.Shapes.Count > 0)
            {
                lineContainer.Shapes.RemoveAt(0);
            }
            
            //foreach (ConnectionGene connection in genome.connectionGenes.Where(gene => gene.enabled))
            //{
            //    LineShape line = new LineShape();
            //    line.Parent = lineContainer;
            //    int inNode = connection.inNodeNum;
            //    if (genome.IsInput(inNode))
            //    {
            //        if (inNode == numberOfRows * numberOfColumns)
            //        {
            //            line.StartPoint = new Point(numberOfColumns * squareDimensions / 2, numberOfRows * (squareDimensions + 2));
            //            line.BorderColor = Color.Blue;
            //        }
            //        else
            //            line.StartPoint = Center(squares[inNode / numberOfColumns, inNode % numberOfColumns]);
            //    }
            //    else if (genome.IsHidden(inNode))
            //    {
            //        line.StartPoint = new Point(450, 450);
            //    }

            //    int outNode = connection.outNodeNum;
            //    if (genome.IsOutput(outNode))
            //    {
            //        int outMovement = outNode - genome.numInputs;
            //        switch (outMovement)
            //        {
            //            case 0:
            //                line.EndPoint = new Point(leftOutputLabel.Left, Center(leftOutputLabel).Y);
            //                break;
            //            case 1:
            //                line.EndPoint = new Point(rightOutputLabel.Left, Center(rightOutputLabel).Y);
            //                break;
            //            case 2:
            //                line.EndPoint = new Point(downOutputLabel.Left, Center(downOutputLabel).Y);
            //                break;
            //            case 3:
            //                line.EndPoint = new Point(rotateOutputLabel.Left, Center(rotateOutputLabel).Y);
            //                break;
            //        }

            //    }
            //    else if (genome.IsHidden(outNode))
            //    {
            //        line.EndPoint = new Point(450, 450);
            //    }
            //    line.BorderColor = connection.weight > 0 ? Color.Green : Color.Red;
            //    line.BorderWidth = 3;
            //}
            lineContainer.Parent.Refresh();
        }

        private Point Center(Control c)
        {
            Point topLeft = c.Location;
            topLeft.X += c.Width / 2;
            topLeft.Y += c.Height / 2;
            return topLeft;
        }
    }
}
