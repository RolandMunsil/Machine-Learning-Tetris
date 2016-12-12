using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tetris.NEAT
{
    //THINGS USED FOR THIS
    //Original paper on NEAT http://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf
    //Updated paper on NEAT  http://nn.cs.utexas.edu/downloads/papers/stanley.phd04.pdf
    //Author's webpage on NEAT http://www.cs.ucf.edu/~kstanley/neat.html
    //Source code to original implementation of NEAT https://github.com/FernandoTorres/NEAT/
    //  Note: I tried to limit how much I looked at this but there were some places where the paper
    //        was not very clear so I had to look at the source code. I mainly only looked at the
    //        offspring generation code.
    //http://stackoverflow.com/questions/31708478/how-to-evolve-weights-of-a-neural-network-in-neuroevolution

    public class NEAT
    {
        //The maximum magnitude of a mutation that changes the weight of a connection
        readonly double weightMutationRange = 2.5;
        //Coefficient representing how important excess genes are in measuring compatibility
        public readonly double c1 = 1;
        //Coefficient for disjoint genes
        public readonly double c2 = 1;
        //Coefficient for average weight difference
        public readonly double c3 = 0.2;
        //Compatibility threshold
        double compatabilityThreshhold = 2.5;
        //Percentage of each species allowed to reproduce
        readonly double survivalThreshhold = 0.3;
        //Probability that a reproduction will only result from mutation and not crossover
        readonly double mutateOnlyProbability = .25;
        //Probability a new node gene will be added to the genome
        readonly double mutateAddNodeProbability = 0.10;
        //Probability a new connection will be added
        readonly double mutateAddLinkProbability = 0.70;
        //Percentage of crossovers allowed to occur between parents of different species
        readonly double interspeciesMatingRate = 0.001;
        //Probability that matching genes will be averaged during crossover (otherwise they will be randomly chosen)
        readonly double mateByAveragingProbability = 0.4;

        readonly double disabledIfEitherDisabledProbability = 0.75;
        //Probability an offspring will be mutated after crossover
        readonly double mutateAfterMatingRate = 0.2;
        //Probability that, if nothing is added to a mutated organism, that its' weights will be mutated
        readonly double mutateWeightsRate = 0.1;
        //Number of networks in the population
        readonly int populationSize = 1000;
        //Maximum number of generations a species is allowed to stay the same fitness before it is removed
        readonly int maximumStagnation = 15;

        readonly int speciesCountUpperBound = 50;

        readonly int speciesCountLowerBound = 20;



        readonly int minGenZeroConnections = 2;
        readonly int maxGenZeroConnections = 4;
        readonly double chanceOfGenZeroAddNode = .1;

        readonly int numRows = 20;
        readonly int numCols = 10;
        public readonly int movesAllowedBetweenTicks = 4;
        readonly int numOutputs = 4;

        int nextSpeciesNumber = 1;
        int nextInnovationNumber = 1;
        int nextNodeNumber;
        Dictionary<Tuple<int, int>, int> connectionInnovations;
        Dictionary<int, int> addNodeInnovations;

        Random rand;
        MathNet.Numerics.Distributions.Normal randNorm;
        public List<Species> allSpecies;
        public int currentGeneration;

        public IEnumerable<Organism> Organisms
        {
            get
            {
                return allSpecies.SelectMany(s => s.members);
            }
        }

        public struct GenerationCreationInfo
        {
            public List<Species> speciesRemovedBecauseStagnation;
            public List<Species> newSpeciesCreated;
            public List<Species> speciesRemovedBecauseNoMembers;
            public int organismsCarriedOver;
            public int organismsCreated;
            public int organismsCreatedFromSameSpeciesMating;
            public int organismsCreatedFromInterspeciesMating;
            public int organismsCreatedFromMutation;
            public int organismsCreatedFromMatingAndMutation;
            public int nodeMutationsDone;
            public int connectionMutationsDone;
            public int connectionMutationsAttempted;
            public int weightMutationsDone;
            public int totalInnovationsCreated;
        }
        public GenerationCreationInfo genCreationInfo;

        public NEAT(int randomSeed = -1)
        {
            if (randomSeed == -1)
                rand = new Random();
            else
                rand = new Random(randomSeed);

            randNorm = new MathNet.Numerics.Distributions.Normal(rand);
            connectionInnovations = new Dictionary<Tuple<int, int>, int>();
            addNodeInnovations = new Dictionary<int, int>();
            allSpecies = new List<Species>();
            genCreationInfo = new GenerationCreationInfo();
        }

        #region XOR Stuff
        public void MakeGenZeroXOR()
        {
            allSpecies = new List<Species>() { new Species(0, null) };
            for (int i = 0; i < populationSize; i++)
            {
                Genome simplestGenome = new Genome(3, 1);
                simplestGenome.AddGene(new ConnectionGene(0, 3, randNorm.Sample(), true, 0));
                simplestGenome.AddGene(new ConnectionGene(1, 3, randNorm.Sample(), true, 1));
                simplestGenome.AddGene(new ConnectionGene(2, 3, randNorm.Sample(), true, 2));
                allSpecies[0].members.Add(new Organism(simplestGenome));
            }
            nextInnovationNumber = 3;
            nextNodeNumber = 4;
        }

        public void EvalGenerationXOR()
        {
            foreach (Species species in allSpecies)
            {
                double maxFitness = 0;
                foreach (Organism organism in species.members)
                {
                    if (organism.fitness == -1)
                        organism.fitness = EvalNeuralNetXOR(organism.neuralNet);
                    maxFitness = Math.Max(maxFitness, organism.fitness);
                }
                if (maxFitness > species.maxFitnessLastImprovedGeneration)
                {
                    species.lastImprovedGeneration = currentGeneration;
                    species.maxFitnessLastImprovedGeneration = maxFitness;
                }
            }
        }

        private double EvalNeuralNetXOR(NeuralNetwork network)
        {
            double sum =
            Math.Abs(network.FeedForward(new[] { 1.0, 1.0,  1.0 })[0] - 0) +
            Math.Abs(network.FeedForward(new[] { 1.0, 0.0,  1.0 })[0] - 1) +
            Math.Abs(network.FeedForward(new[] { 0.0, 1.0,  1.0 })[0] - 1) +
            Math.Abs(network.FeedForward(new[] { 0.0, 0.0,  1.0 })[0] - 0);

            return (4 - sum) * (4 - sum);
        }
        #endregion

        /// <summary>
        /// The first generation will be entirely species with 1 member. Each member will have a few connections and
        /// possibly a hidden node.
        /// </summary>
        public void MakeGenerationZero()
        {
            nextNodeNumber = numRows * numCols + 1 + numOutputs;
            for (int i = 0; i < populationSize; i++)
            {
                Genome g = new Genome(numRows * numCols + 1, numOutputs);

                //Add connections
                int numConnections = rand.Next(minGenZeroConnections, maxGenZeroConnections + 1);
                for (int c = 0; c < numConnections; c++)
                {
                    MutateAddConnection(g);
                }

                //Add hidden node
                if(rand.NextDouble() < chanceOfGenZeroAddNode)
                {
                    MutateAddNode(g);
                }

                Species newSpecies = new Species(nextSpeciesNumber++, g.Clone());
                Organism o = new Organism(g);
                EvaluateOrganism(o);
                newSpecies.members.Add(o);
                allSpecies.Add(newSpecies);
            }
            UpdateLastImprovedGeneration();
            currentGeneration = 0;
            addNodeInnovations.Clear();
            connectionInnovations.Clear();
        }

        #region Mutations
        /// <summary>
        /// Picks a random enabled connection and splits it by adding a new node inbetween the connections input and output.
        /// </summary>
        /// <param name="genome"></param>
        void MutateAddNode(Genome genome)
        {
            //Pick a random enabled connection that is not the bias
            int geneIndex;
            do
            {
                geneIndex = rand.Next(genome.connectionGenes.Count);
            }
            while (genome.connectionGenes[geneIndex].enabled == false || 
                   genome.connectionGenes[geneIndex].inNode == genome.numInputs - 1);

            //Disable the gene
            ConnectionGene toSplit = genome.connectionGenes[geneIndex];
            genome.connectionGenes[geneIndex] = new ConnectionGene(toSplit.inNode, toSplit.outNode, toSplit.weight, false, toSplit.innovationNumber);

            //Determine the innovation numbers of the new connections
            int firstInnovationNum;
            if (!addNodeInnovations.TryGetValue(toSplit.innovationNumber, out firstInnovationNum))
            {
                firstInnovationNum = nextInnovationNumber;
                nextInnovationNumber += 2;
                addNodeInnovations.Add(toSplit.innovationNumber, firstInnovationNum);
            }

            //Create the new genes
            int newNode = nextNodeNumber++;
            genome.AddGene(new ConnectionGene(toSplit.inNode, newNode, 1, true, firstInnovationNum));
            genome.AddGene(new ConnectionGene(newNode, toSplit.outNode, toSplit.weight, true, firstInnovationNum + 1));

            genCreationInfo.nodeMutationsDone++;
        }

        /// <summary>
        /// Adds a connection between two randomly selected nodes. This can take the form of reenabling a connection 
        /// </summary>
        /// <param name="genome"></param>
        public void MutateAddConnection(Genome genome)
        {
            genCreationInfo.connectionMutationsAttempted++;

            //Generate a list of every possible connection.
            //We make a list so that we know when we've tested every possible connection,
            //and so that we don't retry the same connection more than once
            List<Tuple<int, int>> possibleConnections = new List<Tuple<int, int>>();
            foreach(int inNode in Enumerable.Range(0, genome.numInputs).Concat(genome.hiddenNodes))
            {
                foreach (int outNode in Enumerable.Range(genome.numInputs, genome.numOutputs).Concat(genome.hiddenNodes))
                {
                    possibleConnections.Add(new Tuple<int, int>(inNode, outNode));
                }
            }

            //Try random connections until we find one that works.
            Tuple<int, int> workingConnection = null;
            while (possibleConnections.Count > 0)
            {
                //Pick a random connection
                int index = rand.Next(possibleConnections.Count);
                int inNode = possibleConnections[index].Item1;
                int outNode = possibleConnections[index].Item2;

                //Don't allow a node to connect to itself
                if (inNode == outNode)
                {
                    possibleConnections.RemoveAt(index);
                    continue;
                }

                //Check if there's already a gene with the same connection. If it's disabled, replace it with a new gene.
                //Otherwise try another connection
                int dupeIndex = genome.connectionGenes.FindIndex(gene => gene.inNode == inNode && gene.outNode == outNode);
                if (dupeIndex != -1)
                {
                    ConnectionGene theGene = genome.connectionGenes[dupeIndex];
                    if (!theGene.enabled)
                    {
                        genome.connectionGenes.RemoveAt(dupeIndex);
                        workingConnection = possibleConnections[index];
                        break;
                    }
                    else
                    {
                        possibleConnections.RemoveAt(index);
                        continue;
                    }
                }

                //Don't have to worry about loops if connecting from an input or to an output
                if (genome.IsInput(inNode) || genome.IsOutput(outNode))
                {
                    workingConnection = possibleConnections[index];
                    break;
                }
                //Don't allow loops - check if inNode depends on outNode
                else if (!genome.NodeDependsOn(inNode, outNode))
                {
                    workingConnection = possibleConnections[index];
                    break;
                }
                else //This connection doesn't work.
                {
                    possibleConnections.RemoveAt(index);
                }
            }

            //If we found a working connection
            if (workingConnection != null)
            {
                //Determine innovation number of new connection
                int innovationNum;
                if (!connectionInnovations.TryGetValue(workingConnection, out innovationNum))
                {
                    innovationNum = nextInnovationNumber++;
                    connectionInnovations.Add(workingConnection, innovationNum);
                }
                genome.AddGene(new ConnectionGene(workingConnection.Item1, workingConnection.Item2, randNorm.Sample(), true, innovationNum));

                genCreationInfo.connectionMutationsDone++;
            }
        }
        #endregion

        private void EvaluateOrganism(object organism)
        {
            Organism o = (Organism) organism;
            o.fitness = EvaluateNeuralNet(o.neuralNet);
            Interlocked.Increment(ref finishedEvals);
        }

        private int finishedEvals;

        /// <returns>true if there are still species, false if all species were removed due to stagnation</returns>
        public bool MakeNextGeneration()
        {
#if DEBUG
            if(allSpecies.Count == 0 || allSpecies.Sum(s => s.members.Count) == 0)
            {
                throw new InvalidOperationException("There are no organisms.");
            }
#endif
            genCreationInfo = new GenerationCreationInfo
            {
                speciesRemovedBecauseStagnation = new List<Species>(),
                newSpeciesCreated = new List<Species>(),
                speciesRemovedBecauseNoMembers = new List<Species>(),
                organismsCarriedOver = 0,
                organismsCreated = 0,
                organismsCreatedFromSameSpeciesMating = 0,
                organismsCreatedFromInterspeciesMating = 0,
                organismsCreatedFromMutation = 0,
                organismsCreatedFromMatingAndMutation = 0,
                nodeMutationsDone = 0,
                connectionMutationsDone = 0,
                connectionMutationsAttempted = 0,
                weightMutationsDone = 0,
                totalInnovationsCreated = 0,
            };

            //Remove species that have stagnated (i.e. havent improved in a certain number of generations)
            for (int i = 0; i < allSpecies.Count; i++)
            {
                if(currentGeneration - allSpecies[i].lastImprovedGeneration >= maximumStagnation)
                {
                    genCreationInfo.speciesRemovedBecauseStagnation.Add(allSpecies[i]);
                    allSpecies.RemoveAt(i);
                    i--;
                }
            }
            if (allSpecies.Count == 0)
                return false;

            double totalAvgFitness = allSpecies.Sum(s => s.AverageFitness);
            List<Organism> offspring = new List<Organism>();

            //This is to keep track of when all organisms are finished being evaluated.
            finishedEvals = 0;
            int totalEvals = 0;

            double fracPartTotal = 0;
            for (int spIndex = 0; spIndex < allSpecies.Count; spIndex++)
            {
                Species species = allSpecies[spIndex];

                //Calculate number of offspring to generate
                int numOffspring;
                if (spIndex == allSpecies.Count - 1)
                    numOffspring = populationSize - offspring.Count;
                else
                {
                    double numOffSpringDouble = (species.AverageFitness/totalAvgFitness)*populationSize;
                    numOffspring = (int)Math.Floor(numOffSpringDouble);

                    //Account for fractional part
                    fracPartTotal += numOffSpringDouble % 1;
                    if (fracPartTotal >= 1)
                    {
                        numOffspring += 1;
                        fracPartTotal -= 1;
                    }
                }

                //Add the champ
                if (numOffspring > 0 && species.members.Count > 5)
                {
                    Organism champion = species.members[0];
                    foreach (Organism member in species.members)
                    {
                        if (member.fitness > champion.fitness)
                            champion = member;
                    }
                    offspring.Add(champion);
                    genCreationInfo.organismsCarriedOver++;
                    numOffspring--;
                }

                //Only allow top performers to generate offspring
                int numParents = (int)Math.Ceiling(species.members.Count * survivalThreshhold);
                List<Organism> possibleParents = species.members.OrderByDescending(o => o.fitness).Take(numParents).ToList();

                for (int i = 0; i < numOffspring; i++)
                {
                    //Mutate or mate? (always mutate if only 1 member of species)
                    if (possibleParents.Count == 1 || rand.NextDouble() < mutateOnlyProbability)
                    {
                        Genome parentGenome = possibleParents[rand.Next(possibleParents.Count)].genome.Clone();
                        Mutate(parentGenome);
                        Organism newOrg = new Organism(parentGenome);
                        offspring.Add(newOrg);

                        totalEvals++;
                        ThreadPool.QueueUserWorkItem(EvaluateOrganism, newOrg);

                        genCreationInfo.organismsCreated++;
                        genCreationInfo.organismsCreatedFromMutation++;
                    }
                    else //Mate
                    {
                        //Pick parents
                        Organism o1 = RandElementOf(possibleParents);
                        Organism o2;
                        if (allSpecies.Count > 1 && rand.NextDouble() < interspeciesMatingRate)
                        {
                            //Pick a random organism from a random species.
                            Species randSpec;
                            do
                            {
                                randSpec = RandElementOf(allSpecies);
                            } while (randSpec == species);
                            o2 = RandElementOf(randSpec.members);
                            genCreationInfo.organismsCreatedFromInterspeciesMating++;
                        }
                        else
                        {
                            do
                            {
                                o2 = RandElementOf(possibleParents);
                            } while (o1 == o2);
                            genCreationInfo.organismsCreatedFromSameSpeciesMating++;
                        }

                        //Now do the mating
                        Genome child = Mate(o1, o2);
#if DEBUG
                        if (child.connectionGenes.Count == 0) Debugger.Break();
                        if (!child.GenesAreInOrder()) Debugger.Break();
                        child.CheckConnectionsMatchHiddenNodes();
#endif
                        if (rand.Next() < mutateAfterMatingRate)
                        {
                            Mutate(child);
                            genCreationInfo.organismsCreatedFromMatingAndMutation++;
                        }

                        Organism newOrg = new Organism(child);
                        offspring.Add(newOrg);
                        totalEvals++;
                        ThreadPool.QueueUserWorkItem(EvaluateOrganism, newOrg);

                        genCreationInfo.organismsCreated++;
                    }
                }
            }

            genCreationInfo.totalInnovationsCreated = addNodeInnovations.Count + connectionInnovations.Count;

#if DEBUG
            if (offspring.Count != populationSize)
                Debugger.Break();
#endif

            //Now add organisms to species
            FormSpecies(offspring);

            //Adjust compatability threshhold to keep number of species within a certain range
            if (allSpecies.Count > speciesCountUpperBound)
                compatabilityThreshhold += 0.1;
            if (allSpecies.Count < speciesCountLowerBound)
                compatabilityThreshhold -= 0.1;

            //Update last improved generation once organisms finish being evaluated.
            SpinWait.SpinUntil(() => totalEvals == finishedEvals);
            UpdateLastImprovedGeneration();

            addNodeInnovations.Clear();
            connectionInnovations.Clear();
            currentGeneration++;
            return true;
        }

        private void UpdateLastImprovedGeneration()
        {
            foreach (Species species in allSpecies)
            {
#if DEBUG
                if (species.members.Any(o => o.fitness == -1))
                    Debugger.Break();
#endif
                double maxFitness = species.members.Max(o => o.fitness);
                if (maxFitness > species.maxFitnessLastImprovedGeneration)
                {
                    species.lastImprovedGeneration = currentGeneration;
                    species.maxFitnessLastImprovedGeneration = maxFitness;
                }
            }
        }

        private Genome Mate(Organism o1, Organism o2)
        {
            bool averageWeights = rand.NextDouble() < mateByAveragingProbability;

            Genome parent1 = o1.genome;
            Genome parent2 = o2.genome;

            Genome child = new Genome(parent1.numInputs, parent1.numOutputs);

            int i1 = 0;
            int i2 = 0;
            while (true)
            {
                if (i1 == parent1.connectionGenes.Count) //The rest in parent2 are excess
                {
                    for(;i2 < parent2.connectionGenes.Count; i2++)
                        PossiblyAdd(o2, o1, child, parent2.connectionGenes[i2]);
                    break;
                }
                if (i2 == parent2.connectionGenes.Count) // The rest in parent1 are excess
                {
                    for (; i1 < parent1.connectionGenes.Count; i1++)
                        PossiblyAdd(o1, o2, child, parent1.connectionGenes[i1]);
                    break;
                }

                ConnectionGene gene1 = parent1.connectionGenes[i1];
                ConnectionGene gene2 = parent2.connectionGenes[i2];
                if (gene1.innovationNumber < gene2.innovationNumber) //Only in org 1
                {
                    PossiblyAdd(o1, o2, child, gene1);
                    i1++;
                }
                else if (gene2.innovationNumber < gene1.innovationNumber) //Only in org 2
                {
                    PossiblyAdd(o2, o1, child, gene2);
                    i2++;
                }
                else //Shared gene
                {
                    ConnectionGene newGene = new ConnectionGene();
                    newGene.inNode = gene1.inNode;
                    newGene.outNode = gene1.outNode;
                    newGene.innovationNumber = gene1.innovationNumber;
                    newGene.enabled = (gene1.enabled && gene2.enabled) || !(rand.NextDouble() < disabledIfEitherDisabledProbability);
                    if (averageWeights)
                        newGene.weight = (gene1.weight + gene2.weight) / 2;
                    else
                        newGene.weight = rand.NextDouble() < 0.5 ? gene1.weight : gene2.weight;
                    child.AddGene(newGene);

                    i1++;
                    i2++;
                }
            }
            return child;
        }

        private void PossiblyAdd(Organism o1, Organism o2, Genome child, ConnectionGene gene1)
        {
            if (o1.fitness > o2.fitness || (o1.fitness == o2.fitness && rand.NextDouble() < 0.5))
            {
                if (!child.NodeDependsOn(gene1.inNode, gene1.outNode))
                {
                    if (!child.connectionGenes.Exists(g => g.inNode == gene1.inNode && g.outNode == gene1.outNode))
                    {
                        child.AddGene(gene1);
                    }
                }
            }
        }

        private void Mutate(Genome parentGenome)
        {
            if (rand.NextDouble() < mutateAddNodeProbability)
            {
                MutateAddNode(parentGenome);
            }
            else if (rand.NextDouble() < mutateAddLinkProbability)
            {
                MutateAddConnection(parentGenome);
            }
            else if (rand.NextDouble() < mutateWeightsRate) //Do a normal weight mutation
            {
                genCreationInfo.weightMutationsDone++;
                for (int g = 0; g < parentGenome.connectionGenes.Count; g++)
                {
                    ConnectionGene gene = parentGenome.connectionGenes[g];
                    gene.weight += (2*rand.NextDouble() - 1)*weightMutationRange;
                    //Cuz its a struct.
                    parentGenome.connectionGenes[g] = gene;
                }
            }
        }

        double Compatability(Genome genome1, Genome genome2)
        {
            int numDisjoint, numExcess, numMatching;
            double weightDiff;
            Genome.CompatabilityParts(genome1, genome2, out numDisjoint, out numExcess, out weightDiff, out numMatching);

            double n = Math.Max(genome1.connectionGenes.Count, genome2.connectionGenes.Count);
            return (c1 * numExcess + c2 * numDisjoint) / n + c3 * (weightDiff / numMatching);
        }

        private void FormSpecies(List<Organism> offspring)
        {
            //Sort in descending order by max fitness. This way organisms end up in best species they are compatable with.
            allSpecies.Sort((s1, s2) => -s1.MaxFitness.CompareTo(s2.MaxFitness));

            //Select a representative genome for each species and remove all of its organisms
            foreach (Species species in allSpecies)
            {
                species.representativeGenome = RandElementOf(species.members).genome;
                species.members.Clear();
            }

            //Place organisms into species.
            foreach (Organism organism in offspring)
            {
                bool speciesFound = false;
                foreach (Species species in allSpecies)
                {
                    if (Compatability(species.representativeGenome, organism.genome) < compatabilityThreshhold)
                    {
                        species.members.Add(organism);
                        speciesFound = true;
                        break;
                    }
                }
                if (!speciesFound)
                {
                    Species newSpecies = new Species(nextSpeciesNumber++, organism.genome);
                    newSpecies.members.Add(organism);
                    newSpecies.lastImprovedGeneration = currentGeneration;
                    allSpecies.Add(newSpecies);

                    genCreationInfo.newSpeciesCreated.Add(newSpecies);
                }
            }

            //Remove species that didn't get any members
            for (int i = 0; i < allSpecies.Count; i++)
            {
                if (allSpecies[i].members.Count == 0)
                {
                    genCreationInfo.speciesRemovedBecauseNoMembers.Add(allSpecies[i]);
                    allSpecies.RemoveAt(i);
                    i--;
                }
            }

#if DEBUG
            if (allSpecies.Exists(s => s.members.Count == 0))
                throw new Exception();
            if (allSpecies.Sum(s => s.members.Count) != populationSize)
                throw new Exception();
#endif
        }

        long EvaluateNeuralNet(NeuralNetwork network)
        {
            //Pieces should always fall in the same order for consistent evaluation so we give a seed to Random
            Board board = new Board(numRows, numCols, new Random(0));

            long ticksSurvived = 0;
            while(!board.hasLost)
            {
                for(int i = 0; i < movesAllowedBetweenTicks; i++)
                {
                    bool madeMove = NetworkStep(network, board);
                    if (!madeMove)
                        break; //We know it's going to do the same thing for the rest of the ticks.
                }
                board.Tick();
                ticksSurvived++;
            }
            return ticksSurvived + (board.rowsDestroyed * 10);
        }

        internal bool NetworkStep(NeuralNetwork network, Board board)
        {
            //Put block squares onto board
            List<Coordinate> takeOff = new List<Coordinate>(4);
            foreach(Coordinate coord in board.currentBlock.squareCoords)
            {
                Coordinate c = board.currentBlock.ToBoardCoordinates(coord);
                if (c.row >= 0)
                {
                    takeOff.Add(c);
                    board[c.row, c.col] = Board.BLOCK_SQUARES;
                }
            } 
            double[] outputs = network.FeedForward(board.board);
            //Take them back off
            foreach (Coordinate coord in takeOff)
            {
                board[coord.row, coord.col] = Board.EMPTY_SPACE;
            }

            bool triedMoveLeft = outputs[0] > .5;
            bool triedMoveRight = outputs[1] > .5;
            bool triedMoveDown = outputs[2] > .5;
            bool triedRotate = outputs[3] > .5;
            //Left and right movement cancel each other out
            if (triedMoveLeft && triedMoveRight) triedMoveLeft = triedMoveRight = false;
            if (triedMoveLeft)
                board.TryMoveBlockLeft();
            if (triedMoveRight)
                board.TryMoveBlockRight();
            if (triedRotate)
                board.TryRotateBlock();
            if (triedMoveDown)
                board.TryLowerBlock();

            //Return whether the neural net tried to make a move
            return triedMoveLeft || triedMoveRight || triedMoveDown || triedRotate;
        }

        T RandElementOf<T>(IList<T> list)
        {
            return list[rand.Next(list.Count)];
        }
    }
}
