using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.NEAT
{
    public class NEAT
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

        //The maximum magnitude of a mutation that changes the weight of a connection
        readonly double weightMutationRange = 2.5;
        //Coefficient representing how important excess genes are in measuring compatibility
        public readonly double c1 = 1;
        //Coefficient for disjoint genes
        public readonly double c2 = 1;
        //Coefficient for average weight difference
        public readonly double c3 = 0.4;
        //Compatibility threshold
        readonly double compatabilityThreshhold = 2.2;
        //Percentage of each species allowed to reproduce
        readonly double survivalThreshhold = 0.1;
        //Probability that a reproduction will only result from mutation and not crossover
        readonly double mutateOnlyProbability = .25;
        //Probability a new node gene will be added to the genome
        readonly double mutateAddNodeProbability = 0.03;
        //Probability a new connection will be added
        readonly double mutateAddLinkProbability = 0.15;
        //Percentage of crossovers allowed to occur between parents of different species
        readonly double interspeciesMatingRate = 0.001;
        //Probability that matching genes will be averaged during crossover (otherwise they will be randomly chosen)
        readonly double mateByAveragingProbability = 0.4;
        //Probability an offspring will be mutated after crossover
        readonly double mutateAfterMatingRate = 0.8;

        readonly double mutateWeightsRate = 0.9;
        readonly double mutateUniformPerturbWeightRate = 0.9; //Otherwise replace with random value
        
        //Number of networks in the population
        readonly int populationSize = 150;
        //Maximum number of generations a species is allowed to stay the same fitness before it is removed
        readonly int maximumStagnation = 15;


        readonly int genZeroSpeciesSize = 1;
        readonly int minGenZeroConnections = 2;
        readonly int maxGenZeroConnections = 4;
        readonly double chanceOfGenZeroAddNode = .1;

        readonly int numRows = 20;
        readonly int numCols = 10;
        public readonly int movesAllowedBetweenTicks = 4;
        long baseTicksSurvived;
        readonly int numOutputs = 4;

        int nextSpeciesNumber = 1;
        int nextInnovationNumber = 1;
        int nextNodeNumber;
        Dictionary<Tuple<int, int>, int> connectionInnovations;
        //TODO: should this be based on in/out nodes or innovation number?
        Dictionary<int, int> addNodeInnovations;

        Random rand;
        MathNet.Numerics.Distributions.Normal randNorm;
        public List<Species> allSpecies;
        public int currentGeneration;

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
            baseTicksSurvived = CalculateBaseTicksSurvived();
            genCreationInfo = new GenerationCreationInfo();
        }

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
                if (maxFitness > species.maxOrigFitnessLastImprovedGeneration)
                {
                    species.lastImprovedGeneration = currentGeneration;
                    species.maxOrigFitnessLastImprovedGeneration = maxFitness;
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

        public void MakeGenerationZero()
        {
            nextNodeNumber = numRows * numCols + 1 + numOutputs;
            for (int i = 0; i < populationSize / genZeroSpeciesSize; i++)
            {
                Genome g = new Genome(numRows * numCols + 1, numOutputs);

                //Add connections
                int numConnections = rand.Next(minGenZeroConnections, maxGenZeroConnections + 1);
                for (int c = 0; c < numConnections; c++)
                {
                    MutateAddConnection(g);
#if DEBUG
                    if (!g.GenesAreInOrder()) Debugger.Break();
                    g.CheckConnectionsMatchHiddenNodes();
#endif
                }

                //Add hidden node
                if(rand.NextDouble() < chanceOfGenZeroAddNode)
                {
                    MutateAddNode(g);
#if DEBUG
                    if (!g.GenesAreInOrder()) Debugger.Break();
                    g.CheckConnectionsMatchHiddenNodes();
#endif
                }

#if DEBUG
                if (!g.GenesAreInOrder()) Debugger.Break();
                g.CheckConnectionsMatchHiddenNodes();
#endif

                Species newSpecies = new Species(nextSpeciesNumber++, g);
                for(int j = 0; j < genZeroSpeciesSize; j++)
                {
                    newSpecies.members.Add(new Organism(g.Clone()));
                }
                allSpecies.Add(newSpecies);
            }
            currentGeneration = 0;
            addNodeInnovations.Clear();
            connectionInnovations.Clear();
        }

        void MutateAddNode(Genome genome)
        {
            //Pick a random enabled connection that is not the bias
            int geneIndex;
            do
            {
                geneIndex = rand.Next(genome.connectionGenes.Count);
            }
            while (genome.connectionGenes[geneIndex].enabled == false || genome.connectionGenes[geneIndex].inNodeNum == genome.numInputs - 1);

            ConnectionGene toSplit = genome.connectionGenes[geneIndex];
            genome.connectionGenes[geneIndex] = new ConnectionGene(toSplit.inNodeNum, toSplit.outNodeNum, toSplit.weight, false, toSplit.innovationNumber);

            int firstInnovationNum;
            if (!addNodeInnovations.TryGetValue(toSplit.innovationNumber, out firstInnovationNum))
            {
                firstInnovationNum = nextInnovationNumber;
                nextInnovationNumber += 2;
                addNodeInnovations.Add(toSplit.innovationNumber, firstInnovationNum);
            }

            int newNode = nextNodeNumber++;
            ConnectionGene newGene1 = new ConnectionGene(toSplit.inNodeNum, newNode, 1, true, firstInnovationNum);
            ConnectionGene newGene2 = new ConnectionGene(newNode, toSplit.outNodeNum, toSplit.weight, true, firstInnovationNum + 1);
            genome.AddGene(newGene1);
            genome.AddGene(newGene2);
            genCreationInfo.nodeMutationsDone++;
#if DEBUG
            genome.CheckConnectionsMatchHiddenNodes();
#endif
        }

        public void MutateAddConnection(Genome genome)
        {
            //if (genome.HiddenNodes().Count() > 0)
            //    Debugger.Break();

            genCreationInfo.connectionMutationsAttempted++;

            List<Tuple<int, int>> possibleConnections = new List<Tuple<int, int>>();
            foreach(int inNode in Enumerable.Range(0, genome.numInputs).Concat(genome.HiddenNodes()))
            {
                foreach (int outNode in Enumerable.Range(genome.numInputs, genome.numOutputs).Concat(genome.HiddenNodes()))
                {
                    possibleConnections.Add(new Tuple<int, int>(inNode, outNode));
                }
            }

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
                if (genome.connectionGenes.Exists(gene => gene.inNodeNum == inNode && gene.outNodeNum == outNode))
                {
                    int geneIndex = genome.connectionGenes.FindIndex(gene => gene.inNodeNum == inNode && gene.outNodeNum == outNode);
                    ConnectionGene theGene = genome.connectionGenes[geneIndex];
                    if (!theGene.enabled)
                    {
                        genome.connectionGenes.RemoveAt(geneIndex);
                        workingConnection = possibleConnections[index];
                        break;
                        //genome.connectionGenes[geneIndex] = new ConnectionGene(inNode, outNode, randNorm.Sample(), true, theGene.innovationNumber);
                        //genCreationInfo.connectionMutationsDone++;
                        //break;
                    }
                    else
                    {
                        possibleConnections.RemoveAt(index);
                        continue;
                    }
                }

                //Don't allow loops - check if inNode depends on outNode
                if (genome.IsInput(inNode) || genome.IsOutput(outNode))
                {
                    workingConnection = possibleConnections[index];
                    break;
                }
                else if (!genome.NodeDependsOn(inNode, outNode))
                {
                    workingConnection = possibleConnections[index];
                    break;
                }
                else
                {
                    possibleConnections.RemoveAt(index);
                }
            }

            if (workingConnection == null)
            {
                //No connections can be added
            }
            else
            {
                int innovationNum;
                if (!connectionInnovations.TryGetValue(workingConnection, out innovationNum))
                {
                    innovationNum = nextInnovationNumber++;
                    connectionInnovations.Add(workingConnection, innovationNum);
                }
                genome.AddGene(new ConnectionGene(workingConnection.Item1, workingConnection.Item2, randNorm.Sample(), true, innovationNum));
                genCreationInfo.connectionMutationsDone++;
            }
#if DEBUG
            genome.CheckConnectionsMatchHiddenNodes();
#endif
        }

        double Compatability(Genome genome1, Genome genome2)
        {
            int numDisjoint, numExcess, numMatching;
            double weightDiff;
            Genome.CompatabilityParts(genome1, genome2, out numDisjoint, out numExcess, out weightDiff, out numMatching);

            return c1 * numExcess + c2 * numDisjoint + c3 * (weightDiff / numMatching);
        }

        public void EvaluateGeneration()
        {
            foreach(Species species in allSpecies)
            {
                double maxFitness = 0;
                foreach(Organism organism in species.members)
                {
                    if(organism.fitness == -1)
                        organism.fitness = EvaluateNeuralNet(organism.neuralNet);
                    maxFitness = Math.Max(maxFitness, organism.fitness);
                }
                if(maxFitness > species.maxOrigFitnessLastImprovedGeneration)
                {
                    species.lastImprovedGeneration = currentGeneration;
                    species.maxOrigFitnessLastImprovedGeneration = maxFitness;
                }
            }
        }

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

            //foreach (Species species in allSpecies)
            //{
            //    if (species.MaxFitness > 9)
            //        Debugger.Break();
            //}

            for (int i = 0; i < allSpecies.Count; i++)
            {
                if(currentGeneration - allSpecies[i].lastImprovedGeneration >= maximumStagnation)
                {
                    genCreationInfo.speciesRemovedBecauseStagnation.Add(allSpecies[i]);
                    allSpecies.RemoveAt(i);
                    i--;
                }
            }

            double totalAvgFitness = allSpecies.Sum(s => s.AverageFitness);

            double fracPartTotal = 0;

            List<Organism> offspring = new List<Organism>();
            foreach (Species species in allSpecies)
            {
                //Calculate number of offspring to generate
                int numOffspring;
                if (species == allSpecies.Last())
                    numOffspring = populationSize - offspring.Count;
                else
                {
                    double numOffSpringDouble = (species.AverageFitness / totalAvgFitness) * populationSize;
                    fracPartTotal += numOffSpringDouble % 1;

                    numOffspring = (int)Math.Floor(numOffSpringDouble);
                    if (fracPartTotal >= 1)
                    {
                        numOffspring += 1;
                        fracPartTotal--;
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
                List<Organism> possibleParents = TopPerformers(species);

                for (int i = 0; i < numOffspring; i++)
                {
                    //Mutate or mate? (always mutate if only 1 member of species)
                    if (possibleParents.Count == 1 || rand.NextDouble() < mutateOnlyProbability)
                    {
                        Genome parentGenome = possibleParents[rand.Next(possibleParents.Count)].genome.Clone();
                        Mutate(parentGenome);
#if DEBUG
                        if (parentGenome.connectionGenes.Count == 0) Debugger.Break();
                        if (!parentGenome.GenesAreInOrder()) Debugger.Break();
                        parentGenome.CheckConnectionsMatchHiddenNodes();
#endif
                        offspring.Add(new Organism(parentGenome));
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
                            }
                            while (randSpec == species);
                            o2 = RandElementOf(randSpec.members);
                            genCreationInfo.organismsCreatedFromInterspeciesMating++;
                        }
                        else
                        {
                            do
                            {
                                o2 = RandElementOf(possibleParents);
                            }
                            while (o1 == o2);
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

#if DEBUG
                        if (!child.GenesAreInOrder()) Debugger.Break();
                        child.CheckConnectionsMatchHiddenNodes();
#endif

                        offspring.Add(new Organism(child));
                        genCreationInfo.organismsCreated++;
                    }
                }
            }

            genCreationInfo.totalInnovationsCreated = addNodeInnovations.Count + connectionInnovations.Count;

            if (offspring.Count == 0)
                return false;

#if DEBUG
            if (offspring.Count != populationSize)
            {
                Debugger.Break();
                throw new InvalidOperationException();
            }
#endif

            //Now add to existing specices
            FormSpecies(offspring);

            addNodeInnovations.Clear();
            connectionInnovations.Clear();
            currentGeneration++;
            return true;
        }

        private List<Organism> TopPerformers(Species species)
        {
#if DEBUG
            if (species.members.OrderByDescending(o => o.fitness).Last().fitness > species.members.OrderByDescending(o => o.fitness).First().fitness)
                throw new Exception();
#endif
            int numOrganismsToReturn = (int)Math.Ceiling(species.members.Count * survivalThreshhold);
            return species.members.OrderByDescending(o => o.fitness).Take(numOrganismsToReturn).ToList();
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
                    newGene.inNodeNum = gene1.inNodeNum;
                    newGene.outNodeNum = gene1.outNodeNum;
                    newGene.innovationNumber = gene1.innovationNumber;
                    newGene.enabled = (gene1.enabled && gene2.enabled) ? true : !(rand.NextDouble() < 0.75);
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
                if (!child.NodeDependsOn(gene1.inNodeNum, gene1.outNodeNum))
                {
                    if (!child.connectionGenes.Exists(g => g.inNodeNum == gene1.inNodeNum && g.outNodeNum == gene1.outNodeNum))
                    {
                        child.AddGene(gene1);
                    }
                    //else
                    //    Debugger.Break();
                }
                //else
                //    Debugger.Break();
            }
        }

        private void Mutate(Genome parentGenome)
        {
            if (rand.NextDouble() < mutateAddNodeProbability)
            {
                MutateAddNode(parentGenome);
            }
            //We do AddLink second because there's a chance it won't work. This leads to more added nodes (which means more opportunities for added links)
            else if (rand.NextDouble() < mutateAddLinkProbability)
            {
                MutateAddConnection(parentGenome);
            }
            else if (rand.NextDouble() < mutateWeightsRate)//Do a normal weight mutation
            {
                genCreationInfo.weightMutationsDone++;
                for (int g = 0; g < parentGenome.connectionGenes.Count; g++)
                {
                    ConnectionGene gene = parentGenome.connectionGenes[g];
                    //if (rand.NextDouble() < mutateUniformPerturbWeightRate)
                    //{
                        //gene.weight *= (rand.NextDouble() + .5);
                        //if (rand.NextDouble() < mutateWeightFlipSignRate) gene.weight *= -1;
                        gene.weight += (2 * rand.NextDouble() - 1) * weightMutationRange;

                    //}
                    //else
                    //{
                    //    gene.weight = randNorm.Sample();
                    //}
                    //Cuz its a struct.
                    parentGenome.connectionGenes[g] = gene;
                }
            }
        }

        private void FormSpecies(List<Organism> offspring)
        {
            allSpecies = allSpecies.OrderByDescending(s => s.MaxFitness).ToList();
            foreach (Species species in allSpecies)
            {
                species.representativeGenome = RandElementOf(species.members).genome;
                species.members.Clear();
            }
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

        //Calculate how many ticks a useless neural network will survive.
        long CalculateBaseTicksSurvived()
        {
            NeuralNetwork net = new NeuralNetwork(new Genome(numRows * numCols + 1, numOutputs));
            return EvaluateNeuralNet(net);
        }

        long EvaluateNeuralNet(NeuralNetwork network)
        {
            //Pieces should always fall in the same order for consistent evaluation
            Board board = new Board(numRows, numCols, new Random(0));
            long ticksSurvived = 0;
            while(!board.hasLost)
            {
                for(int i = 0; i < movesAllowedBetweenTicks; i++)
                {
                    //Construct inputs
                    NetworkStep(network, board);
                }
                board.Tick();
                ticksSurvived++;
            }
            //return (int)(Math.Pow((ticksSurvived / baseTicksSurvived), 2) * baseTicksSurvived);
            return ticksSurvived;
        }

        internal void NetworkStep(NeuralNetwork network, Board board)
        {           
            //Put block squares onto board
            foreach(Coordinate coord in board.currentBlock.squareCoords)
            {
                var c = board.currentBlock.toBoardCoordinates(coord);
                if (c.row >= 0)
                    board[c.row, c.col] = Board.BLOCK_SQUARES;
            } 
            double[] outputs = network.FeedForward(board.board);
            //Take them back off
            foreach (Coordinate coord in board.currentBlock.squareCoords)
            {
                var c = board.currentBlock.toBoardCoordinates(coord);
                if(c.row >= 0)
                    board[c.row, c.col] = Board.EMPTY_SPACE;
            }

            bool triedMoveLeft = outputs[0] > .5;
            bool triedMoveRight = outputs[1] > .5;
            bool triedMoveDown = outputs[2] > .5;
            bool triedRotate = outputs[3] > .5;
            if (triedMoveLeft && triedMoveRight) triedMoveLeft = triedMoveRight = false;
            if (triedMoveLeft)
                board.TryMoveBlockLeft();
            if (triedMoveRight)
                board.TryMoveBlockRight();
            if (triedRotate)
                board.TryRotateBlock();
            if (triedMoveDown)
                board.TryLowerBlock();
        }

        T RandElementOf<T>(IList<T> list)
        {
            return list[rand.Next(list.Count)];
        }
    }
}
