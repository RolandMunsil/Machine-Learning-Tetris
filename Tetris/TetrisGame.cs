﻿using System;
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
using MathNet.Numerics.Statistics;
using System.Threading.Tasks;

namespace Tetris
{
    public partial class TetrisGame : Form
    {
        /// <summary>
        /// Is a game currently being played?
        /// </summary>
        bool playing;

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

        /// <summary>
        /// Container for the connections between nodes in the visualization
        /// </summary>
        ShapeContainer lineContainer;

        /// <summary>
        /// The hidden nodes in the visualization of the neural network
        /// </summary>
        Dictionary<int, Square> netVizNodes;

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
            netVizNodes = new Dictionary<int, Square>();
        }

        /// <summary>
        /// Resets the game
        /// </summary>
        private void ResetGame()
        {
            CreateSquares();
            board = new Board(numberOfRows, numberOfColumns, new Random());

            tickTimer.Enabled = true;
            playing = true;
        }

        private void tickTimer_Tick(object sender, EventArgs e)
        {
            board.Tick();
            UpdateBoard();
            rowsCleared.Text = board.rowsDestroyed.ToString();
            score.Text = board.score.ToString();
        }

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
                    Square square = new Square();
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
                        if (squares[row, col].Color != board.lockedBlocksColors[row, col])
                            squares[row, col].Color = board.lockedBlocksColors[row, col];
                    }
                    else
                    {
                        if (squares[row, col].Color != Color.PeachPuff)
                            squares[row, col].Color = Color.PeachPuff;
                    }
                }
            }

            // then display the current block
            foreach (Coordinate squareCoord in board.currentBlock.squareCoords)
            {
                Coordinate coord = board.currentBlock.ToBoardCoordinates(squareCoord);
                if (coord.col >= 0 && coord.col < numberOfColumns
                        && coord.row >= 0 && coord.row < numberOfRows)
                {
                    squares[coord.row, coord.col].Color = board.currentBlock.color;
                }
            }

            lineContainer.Refresh();
        }

        #endregion GUI

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
            if (playing)
            {
                switch (e.KeyChar)
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

        private void learnButton_Click(object sender, EventArgs e)
        {
            new Thread(Learn).Start();
        }

        #region Other (commented out) Learn methods
        //private void Learn()
        //{
        //    board = new Board(20, 10, new Random());
        //    RulesBasedPlayer player = new RulesBasedPlayer();
        //    while (!board.hasLost)
        //    {
        //        for (int i = 0; i < 4; i++)
        //        {
        //            //if (board.currentBlock.topLeft.row == 18)
        //            //    Debugger.Break();

        //            //Construct inputs
        //            switch (player.DecideMove(board))
        //            {
        //                case RulesBasedPlayer.Move.Left:
        //                    board.TryMoveBlockLeft();
        //                    break;
        //                case RulesBasedPlayer.Move.Right:
        //                    board.TryMoveBlockRight();
        //                    break;
        //                case RulesBasedPlayer.Move.Down:
        //                    board.TryLowerBlock();
        //                    break;
        //                case RulesBasedPlayer.Move.Rotate:
        //                    board.TryRotateBlock();
        //                    break;
        //            }
        //            this.Invoke(new Action(UpdateBoard));
        //            Thread.Sleep(5);
        //        }
        //        board.Tick();
        //        this.Invoke(new Action(UpdateBoard));
        //        //Thread.Sleep(1000);
        //    }
        //}

        //private void Learn()
        //{
        //    List<int> generationsFound = new List<int>();
        //    List<int> hiddenNodesInSolutions = new List<int>();
        //    List<int> enabledConnectionsInSolutions = new List<int>();
        //    int numSolutionsFound = 0;

        //    Parallel.For(0, 100, delegate (int i)
        //    //for (int i = 0; i < 100; i++)
        //    {
        //        int seed = Environment.TickCount;
        //        NEAT.NEAT xorNeat = new NEAT.NEAT();
        //        xorNeat.MakeGenZeroXOR();
        //        xorNeat.EvalGenerationXOR();

        //        while (true)
        //        {
        //            bool succeeded = xorNeat.MakeNextGeneration();
        //            if (!succeeded || xorNeat.currentGeneration > 200)
        //            {
        //                AddLineToTextBox("FAILED to find solution.");
        //                //Thread.Sleep(1000);
        //                break;
        //            }
        //            xorNeat.EvalGenerationXOR();

        //            //var originalFitnesses = xorNeat.allSpecies.SelectMany(s => s.members).Select(o => o.fitness);
        //            ////adjFitnesses = xorNeat.allSpecies.SelectMany(s => s.members).Select(o => o.fitness);
        //            //AddLineToTextBox($"Gen {xorNeat.currentGeneration}: avg orig fitness {originalFitnesses.Average()}, max {originalFitnesses.Max()}");
        //            ////AddLineToTextBox($"Gen {xorNeat.currentGeneration}: avg adj  fitness {adjFitnesses.Average()}, max {adjFitnesses.Max()}");
        //            //AddLineToTextBox($"    Mutated organisms: {xorNeat.genCreationInfo.organismsCreatedFromMutation + xorNeat.genCreationInfo.organismsCreatedFromMatingAndMutation}");
        //            //AddLineToTextBox($"    Node mutations: {xorNeat.genCreationInfo.nodeMutationsDone}");
        //            //double percentConnectionMutationsSucceeded = 100 * xorNeat.genCreationInfo.connectionMutationsDone / (double)xorNeat.genCreationInfo.connectionMutationsAttempted;
        //            //AddLineToTextBox($"    Connection mutations: {xorNeat.genCreationInfo.connectionMutationsDone} ({xorNeat.genCreationInfo.connectionMutationsAttempted} attempted)");
        //            //AddLineToTextBox($"    Species created: {xorNeat.genCreationInfo.newSpeciesCreated.Count}");
        //            //AddLineToTextBox($"    Species removed: {xorNeat.genCreationInfo.speciesRemovedBecauseStagnation.Count}+{xorNeat.genCreationInfo.speciesRemovedBecauseNoMembers.Count}");
        //            //String speciesStr = "[" + String.Join("][", xorNeat.allSpecies.Select(s => s.members.Count)) + "]";
        //            //AddLineToTextBox($"    {xorNeat.allSpecies.Count} species: {speciesStr}");
        //            //Thread.Sleep(10);
        //            bool solutionFound = false;
        //            foreach (Organism o in xorNeat.allSpecies.SelectMany(s => s.members))
        //            {
        //                solutionFound =
        //                    o.neuralNet.FeedForward(new[] { 1.0, 1.0, 1.0 })[0] < 0.5 &&
        //                    o.neuralNet.FeedForward(new[] { 1.0, 0.0, 1.0 })[0] >= 0.5 &&
        //                    o.neuralNet.FeedForward(new[] { 0.0, 1.0, 1.0 })[0] >= 0.5 &&
        //                    o.neuralNet.FeedForward(new[] { 0.0, 0.0, 1.0 })[0] < 0.5;
        //                if (solutionFound)
        //                {
        //                    lock (textBox1)
        //                    {
        //                        AddLineToTextBox($"!!! Found solution @ gen {xorNeat.currentGeneration} !!!");
        //                        AddLineToTextBox($"Solution: {o.genome}");
        //                        numSolutionsFound++;
        //                        generationsFound.Add(xorNeat.currentGeneration);
        //                        hiddenNodesInSolutions.Add(o.genome.HiddenNodes().Count());
        //                        enabledConnectionsInSolutions.Add(o.genome.connectionGenes.Count(g => g.enabled));
        //                    }

        //                    //Thread.Sleep(1000);
        //                    break;
        //                }
        //            }
        //            if (solutionFound)
        //                break;
        //        }
        //    }
        //    );
        //    AddLineToTextBox("");
        //    AddLineToTextBox($"{numSolutionsFound}% success rate.");
        //    AddLineToTextBox($"Avg gen found {generationsFound.Average()} (stdev {Statistics.StandardDeviation(generationsFound.Select(i => (double)i))}), max {generationsFound.Max()}");
        //    AddLineToTextBox($"Avg hidden nodes {hiddenNodesInSolutions.Average()} (stdev {Statistics.StandardDeviation(hiddenNodesInSolutions.Select(i => (double)i))}), max {hiddenNodesInSolutions.Max()}");
        //    AddLineToTextBox($"Avg enabled connections {enabledConnectionsInSolutions.Average()} (stdev {Statistics.StandardDeviation(enabledConnectionsInSolutions.Select(i => (double)i))}), max {enabledConnectionsInSolutions.Max()}");
        //}
        #endregion

        private NEAT.NEAT neat;
        private void Learn()
        {
            neat = new NEAT.NEAT();
            neat.MakeGenerationZero();

            while(true)
            {
                lock (neat)
                {
                    bool success = neat.MakeNextGeneration();
                    if (!success) throw new Exception();

                    AddLineToTextBox($"Generation {neat.currentGeneration}");
                    AddLineToTextBox($"    Max fitness: {neat.Organisms.Max(o => o.fitness)}");
                    AddLineToTextBox($"    Avg fitness: {neat.Organisms.Average(o => o.fitness)}");
                    AddLineToTextBox($"    Num species: {neat.allSpecies.Count}");
                    this.Invoke(new Action<Organism>(DrawNeuralNet), neat.Organisms.OrderByDescending(o => o.fitness).First());
                }
            }
            
        }

        private void AddLineToTextBox(String str)
        {
            textBox1.Invoke(new Action(() => textBox1.AppendText(str + "\r\n")));
        }

        private void DrawNeuralNet(Organism organism)
        {
            while (lineContainer.Shapes.Count > 0)
            {
                lineContainer.Shapes.RemoveAt(0);
            }
            foreach(Square s in netVizNodes.Values)
            {
                s.Dispose();
            }
            netVizNodes.Clear();

            Genome genome = organism.genome;
            if (genome.hiddenNodes.Count > 0)
            {
                //Figure out positions of hidden layers
                List<List<int>> layers = new List<List<int>>();
                //layer 0
                layers.Add(new List<int>());

                foreach (var node in organism.neuralNet.nonInputNodes)
                {
                    if (genome.IsOutput(node.number))
                        continue;
                    int maxLayer = 0;
                    foreach(int sourceNodeNum in node.sourceNodes)
                    {
                        if (genome.IsInput(sourceNodeNum))
                            continue;
                        for (int layerNum = 0; layerNum < layers.Count; layerNum++)
                        {
                            if (layers[layerNum].Contains(sourceNodeNum))
                                maxLayer = Math.Max(maxLayer, layerNum+1);
                        }
                    }
                    if (maxLayer > layers.Count)
                        throw new Exception();
                    if(maxLayer == layers.Count)
                    {
                        layers.Add(new List<int>());
                    }
                    layers[maxLayer].Add(node.number);
                }

                ////TODO: add any hidden nodes that were removed when creating neural network
                //IEnumerable<int> uselessNodes = organism.genome.HiddenNodes().Except(layers.SelectMany(list => list));
                //foreach (int node in uselessNodes)
                //{
                //    organism.genome.GetInputsTo(node);
                //}

                double minLayerX = squareDimensions * numberOfColumns;
                double maxLayerX = leftOutputLabel.Left;
                double minNodeY = 0;
                double maxNodeY = squareDimensions * numberOfRows;

                for (int layerNum = 0; layerNum < layers.Count; layerNum++)
                {
                    double div = layers.Count + 1;
                    double x = minLayerX + (((layerNum + 1) / div) * (maxLayerX - minLayerX));
                    for (int i = 0; i < layers[layerNum].Count; i++)
                    {
                        double div2 = layers[layerNum].Count + 1;
                        double y = minNodeY + (((i + 1) / div2) * (maxNodeY - minNodeY));

                        Square square = new Square();
                        square.Width = 15;
                        square.Height = 15;
                        square.Parent = gameWindow;
                        square.Top = (int)(y + 7.5);
                        square.Left = (int)(x + 7.5);
                        square.Color = Color.Black;

                        netVizNodes.Add(layers[layerNum][i], square);
                    }
                }
            }

            //Make bias node
            Point biasNodeCenter = new Point((int)(numberOfColumns * squareDimensions / 2.0), numberOfRows * (squareDimensions + 2));

            Square biasSquare = new Square();
            biasSquare.Width = 15;
            biasSquare.Height = 15;
            biasSquare.Parent = gameWindow;
            biasSquare.Top = (int)(biasNodeCenter.Y - 7.5);
            biasSquare.Left = (int)(biasNodeCenter.X - 7.5);
            biasSquare.Color = Color.Black;
            netVizNodes.Add(numberOfRows * numberOfColumns, biasSquare);

            foreach (ConnectionGene connection in genome.connectionGenes.Where(gene => gene.enabled))
            {
                LineShape line = new LineShape();
                
                int inNode = connection.inNode;
                if (genome.IsInput(inNode))
                {
                    if (inNode == numberOfRows * numberOfColumns)
                    {
                        line.StartPoint = biasNodeCenter;
                        line.BorderColor = Color.Blue;
                    }
                    else
                        line.StartPoint = Center(squares[inNode / numberOfColumns, inNode % numberOfColumns]);
                }
                else if (genome.IsHidden(inNode))
                {
                    if (!netVizNodes.ContainsKey(inNode))
                        continue; //Node is useless.
                    line.StartPoint = Center(netVizNodes[inNode]);
                }
                

                int outNode = connection.outNode;
                if (genome.IsOutput(outNode))
                {
                    int outMovement = outNode - genome.numInputs;
                    switch (outMovement)
                    {
                        case 0:
                            line.EndPoint = new Point(leftOutputLabel.Left, Center(leftOutputLabel).Y);
                            break;
                        case 1:
                            line.EndPoint = new Point(rightOutputLabel.Left, Center(rightOutputLabel).Y);
                            break;
                        case 2:
                            line.EndPoint = new Point(downOutputLabel.Left, Center(downOutputLabel).Y);
                            break;
                        case 3:
                            line.EndPoint = new Point(rotateOutputLabel.Left, Center(rotateOutputLabel).Y);
                            break;
                    }

                }
                else if (genome.IsHidden(outNode))
                {
                    if (!netVizNodes.ContainsKey(outNode))
                        continue; //Node is useless.
                    line.EndPoint = Center(netVizNodes[outNode]);
                }
                line.BorderColor = connection.weight > 0 ? Color.Green : Color.Red;
                line.BorderWidth = 2;
                line.Parent = lineContainer;
            }
            foreach (Square s in netVizNodes.Values)
            {
                s.BringToFront();
            }

            lineContainer.Parent.Refresh();
        }

        private void SetText(Label label, String text)
        {
            label.Invoke(new Action(() => label.Text = text));
        }

        private Point Center(Control c)
        {
            Point topLeft = c.Location;
            topLeft.X += c.Width / 2;
            topLeft.Y += c.Height / 2;
            return topLeft;
        }

        private void topPlayButton_Click(object sender, EventArgs e)
        {
            new Thread(ShowTop).Start();
        }

        private void ShowTop()
        {
            //Wait for a generation to be finished before visualizing the top organism playing
            lock (neat)
            {
                Species bestSpecies = neat.allSpecies.OrderByDescending(s => s.MaxFitness).First();
                Organism bestOrganism = bestSpecies.members.OrderByDescending(o => o.fitness).First();

                SetText(genLabel, $"Generation {neat.currentGeneration}");
                SetText(speciesLabel, $"Species #{bestSpecies.speciesNumber} ({neat.allSpecies.Count} tot)");
                SetText(organismLabel, $"Organism {bestSpecies.members.IndexOf(bestOrganism) + 1}/{bestSpecies.members.Count}");
                SetText(fitnessLabel, $"Fitness: {bestOrganism.fitness}");

                this.Invoke(new Action<Organism>(DrawNeuralNet), bestOrganism);

                board = new Board(20, 10, new Random(0));
                while (!board.hasLost)
                {
                    for (int i = 0; i < neat.movesAllowedBetweenTicks; i++)
                    {
                        neat.NetworkStep(bestOrganism.neuralNet, board);
                        this.Invoke(new Action(UpdateBoard));
                        Thread.Sleep(10);
                    }
                    board.Tick();
                    this.Invoke(new Action(UpdateBoard));
                    Thread.Sleep(10);
                }
                //Clear the board
                board = new Board(20, 10, new Random(0));
                this.Invoke(new Action(UpdateBoard));
            }
        }
    }
}
