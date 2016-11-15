using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    class NEAT
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

        readonly int populationSize = 500;
        readonly int minGenZeroConnections = 2;
        readonly int maxGenZeroConnections = 4;
        readonly double chanceOfGenZeroAddNode = .1;
        readonly double c1 = 1;
        readonly double c2 = 1;
        readonly double c3 = 0.4;
        readonly double compatabilityThreshhold = 3.0;
        readonly double mutationRate = .25;
        readonly double mutateAddNewNodeRate = 0.03;
        readonly double mutateAddNewConnectionRate = 0.2;
        readonly double mutateWeightsRate = 0.8;
        readonly double mutateUniformPerturbWeightRate = 0.9; //Otherwise replace with random value
        readonly double mutateWeightFlipSignRate = 0.01;
        readonly double interspeciesMateRate = 0.001;
        readonly double disabledIfEitherParentDisabledRate = 0.75;
        readonly double matingWeightAverageRate = 0.6;
        readonly double survivalThreshhold = 0.4; //The percent of a species that will get to reproduce.

        readonly int numRows = 20;
        readonly int numCols = 10;
        public readonly int movesAllowedBetweenTicks = 4;
        long baseTicksSurvived;
        readonly int numOutputs = 4;

        int nextSpeciesNumber = 1;
        int nextInnovationNumber = 1;
        Dictionary<Tuple<int, int>, int> connectionInnovations;
        //TODO: should this be based on in/out nodes or innovation number?
        Dictionary<int, int> addNodeInnovations;

        Random rand;
        MathNet.Numerics.Distributions.Normal randNorm;
        public List<Species> allSpecies;
        public int currentGeneration;

        public NEAT()
        {
            rand = new Random();
            randNorm = new MathNet.Numerics.Distributions.Normal(rand);
            connectionInnovations = new Dictionary<Tuple<int, int>, int>();
            addNodeInnovations = new Dictionary<int, int>();
            allSpecies = new List<Species>();
            baseTicksSurvived = CalculateBaseTicksSurvived();
        }

        public void MakeGenerationZero()
        {
            for (int i = 0; i < populationSize; i++)
            {
                Genome g = new Genome(numRows * numCols + 1, numOutputs, 0);

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

                //Create new species
                //bool foundSpecies = false;
                //foreach (Species species in allSpecies)
                //{
                //    if(Distance(species.representativeGenome, g) < compatabilityThreshhold)
                //    {
                //        species.members.Add(new Organism(g));
                //        foundSpecies = true;
                //        break;
                //    }
                //}
                //if(!foundSpecies)
                //{
                Species s = new Species();
                s.speciesNumber = nextSpeciesNumber++;
                s.representativeGenome = g;
                s.members = new List<Organism>();
                s.members.Add(new Organism(g));
                allSpecies.Add(s);
                //}
            }
            currentGeneration = 0;
        }

        void MutateAddNode(Genome genome)
        {
            //Pick a random connection
            var geneIndex = rand.Next(genome.connectionGenes.Count);
            ConnectionGene toSplit = genome.connectionGenes[geneIndex];
            genome.connectionGenes[geneIndex] = new ConnectionGene(toSplit.inNodeNum, toSplit.outNodeNum, toSplit.weight, false, toSplit.innovationNumber);

            int firstInnovationNum;
            if (!addNodeInnovations.TryGetValue(toSplit.innovationNumber, out firstInnovationNum))
            {
                firstInnovationNum = nextInnovationNumber;
                nextInnovationNumber += 2;
                addNodeInnovations.Add(toSplit.innovationNumber, firstInnovationNum);
            }

            int newNode = genome.numInputs + genome.numOutputs + genome.numHiddenNodes++;
            ConnectionGene newGene1 = new ConnectionGene(toSplit.inNodeNum, newNode, 1, true, firstInnovationNum);
            ConnectionGene newGene2 = new ConnectionGene(newNode, toSplit.outNodeNum, toSplit.weight, true, firstInnovationNum + 1);
            genome.connectionGenes.Add(newGene1);
            genome.connectionGenes.Add(newGene2);
        }

        void MutateAddConnection(Genome genome)
        {
            //TODO: Only do one random by picking a random input/output and then only selecting
            //from vaild outputs/inputs?

            int inNode = -1, outNode = -1;
            bool workingConnectionFound = false;
            while (!workingConnectionFound)
            {
                //Generate a random connection
                inNode = rand.Next(genome.numInputs + genome.numHiddenNodes);
                if (inNode >= genome.numInputs) inNode += genome.numOutputs;

                outNode = genome.numInputs + rand.Next(genome.numOutputs + genome.numHiddenNodes);

                //Don't allow a node to connect to itself
                if (inNode == outNode)
                    continue;

                //Don't allow recreation of an existing connection
                if (genome.connectionGenes.Exists(gene => gene.inNodeNum == inNode && gene.outNodeNum == outNode))
                    continue;

                //Don't allow loops - check if inNode depends on outNode
                if (genome.IsInput(inNode) || genome.IsOutput(outNode))
                    workingConnectionFound = true;
                else if (!genome.NodeDependsOn(inNode, outNode))
                {
                    workingConnectionFound = true;
                }
            }

            int innovationNum;
            if(!connectionInnovations.TryGetValue(new Tuple<int, int>(inNode, outNode), out innovationNum))
            {
                innovationNum = nextInnovationNumber++;
                connectionInnovations.Add(new Tuple<int, int>(inNode, outNode), innovationNum);
            }
            genome.connectionGenes.Add(new ConnectionGene(inNode, outNode, randNorm.Sample(), true, innovationNum));
        }

        double Distance(Genome genome1, Genome genome2)
        {
            List<ConnectionGene> moreGenes;
            List<ConnectionGene> lessGenes;
            if(genome1.connectionGenes.Count > genome2.connectionGenes.Count)
            {
                moreGenes = genome1.connectionGenes;
                lessGenes = genome2.connectionGenes;
            }
            else
            {
                moreGenes = genome2.connectionGenes;
                lessGenes = genome1.connectionGenes;
            }

            //Calculate average weight diff
            double avgWeightDiff = 0;
            int numSharedGenes = lessGenes.Count;
            for(int i = 0; i < lessGenes.Count; i++)
            {
                if (moreGenes[i].innovationNumber != lessGenes[i].innovationNumber)
                    numSharedGenes = i;
                else
                    avgWeightDiff += Math.Abs(moreGenes[i].weight - lessGenes[i].weight);
            }
            avgWeightDiff /= numSharedGenes;

            //Shortcut if there's no disjoint or excess
            if (moreGenes.Count == lessGenes.Count && numSharedGenes == moreGenes.Count)
                return c3 * avgWeightDiff;

            //Calculate # disjoint and excess
            int highestInnovMore = moreGenes[moreGenes.Count - 1].innovationNumber;
            int highestInnovLess = lessGenes[lessGenes.Count - 1].innovationNumber;
            if (highestInnovMore > highestInnovLess)
            {
                int numExcess = moreGenes.Count((gene) => gene.innovationNumber > highestInnovLess);
                int numDisjoint = moreGenes.Count - (numExcess + numSharedGenes);
                numDisjoint += lessGenes.Count - numSharedGenes;
                return (c1 * numExcess + c2 * numDisjoint) / moreGenes.Count + c3 * avgWeightDiff;
            }
            else
            {
                int numExcess = lessGenes.Count((gene) => gene.innovationNumber > highestInnovMore);
                int numDisjoint = lessGenes.Count - (numExcess + numSharedGenes);
                numDisjoint += moreGenes.Count - numSharedGenes;
                return (c1 * numExcess + c2 * numDisjoint) / moreGenes.Count + c3 * avgWeightDiff;
            }
        }

        public void EvaluateGeneration()
        {
            foreach(Species species in allSpecies)
            {
                foreach(Organism organism in species.members)
                {
                    if(organism.originalFitness == -1)
                        organism.originalFitness = EvaluateNeuralNet(organism.neuralNet);
                    organism.fitness = organism.originalFitness / species.members.Count;
                }
            }
        }

        public void MakeNextGeneration()
        {
            //If a species has >5 members, the best organism should just be cloned.
            double totalAverageFitness = allSpecies.Sum(s => s.AverageFitness);

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
                    double numOffSpringDouble = (species.AverageFitness / totalAverageFitness) * populationSize;
                    fracPartTotal += numOffSpringDouble % 1;

                    numOffspring = (int)Math.Floor(numOffSpringDouble);
                    if (fracPartTotal >= 1)
                    {
                        numOffspring += 1;
                        fracPartTotal--;
                    }
                }

                //Add the champ
                if (species.members.Count > 5)
                {
                    Organism champion = species.members[0];
                    foreach (Organism member in species.members)
                    {
                        if (member.fitness > champion.fitness)
                            champion = member;
                    }
                    offspring.Add(champion);
                    numOffspring--;
                }

                //Only allow top performers to generate offspring
                List<Organism> possibleParents = TopPerformers(species);

                for (int i = 0; i < numOffspring; i++)
                {
                    //Mutate or mate? (always mate if only 1 member of species)
                    if (species.members.Count == 1 || rand.NextDouble() < mutationRate)
                    {
                        Genome parentGenome = possibleParents[rand.Next(possibleParents.Count)].genome.Clone();
                        Mutate(parentGenome);
                        if (parentGenome.connectionGenes.Count == 0) Debugger.Break();
                        offspring.Add(new Organism(parentGenome));
                    }
                    else //Mate
                    {
                        //Pick parents
                        Organism o1 = RandElementOf(possibleParents);
                        Organism o2;
                        //Don't do interspecies mating as all species do not share the same basic innovations
                        //if (allSpecies.Count > 1 && rand.NextDouble() < interspeciesMateRate)
                        //{
                        //    //Pick a random organism from a random species.
                        //    Species randSpec;
                        //    do
                        //    {
                        //        randSpec = RandElementOf(allSpecies);
                        //    }
                        //    while (randSpec == species);
                        //    o2 = RandElementOf(randSpec.members);
                        //}
                        //else
                        //{
                            do
                            {
                                o2 = RandElementOf(possibleParents);
                            }
                            while (o1 == o2);
                        //}

                        //Now do the mating
                        Genome child = Mate(o1, o2);
                        if (child.connectionGenes.Count == 0) Debugger.Break();
                        offspring.Add(new Organism(child));
                    }
                }
            }

            if (offspring.Count != populationSize)
                throw new InvalidOperationException();

            //Now add to existing specices
            FormSpecies(offspring);

            addNodeInnovations.Clear();
            connectionInnovations.Clear();
            currentGeneration++;
        }

        private List<Organism> TopPerformers(Species species)
        {
            List<Organism> possibleParents = new List<Organism>(species.members);
            possibleParents.Sort((x, y) => x.fitness.CompareTo(y.fitness));
            if (possibleParents.Last().fitness > possibleParents[0].fitness)
                throw new Exception();
            possibleParents = possibleParents.Take((int)Math.Ceiling(possibleParents.Count * survivalThreshhold)).ToList();
            return possibleParents;
        }

        private Genome Mate(Organism o1, Organism o2)
        {
            Genome parent1 = o1.genome;
            Genome parent2 = o2.genome;

            Genome child = new Genome(parent1.numInputs, parent1.numOutputs, Int32.MinValue);
            child.numInputs = parent1.numInputs;
            child.numOutputs = parent1.numOutputs;

            //Sharedgenes
            int maxSharedGenes = Math.Min(parent1.connectionGenes.Count, parent2.connectionGenes.Count);
            int numSharedGenes = maxSharedGenes;
            for (int j = 0; j < maxSharedGenes; j++)
            {
                ConnectionGene p1gene = parent1.connectionGenes[j];
                ConnectionGene p2gene = parent2.connectionGenes[j];
                if (p1gene.innovationNumber != p2gene.innovationNumber)
                {
                    numSharedGenes = j;
                    break;
                }
                else
                {
                    ConnectionGene newGene = new ConnectionGene();
                    newGene.inNodeNum = p1gene.inNodeNum;
                    newGene.outNodeNum = p1gene.outNodeNum;
                    newGene.innovationNumber = p1gene.innovationNumber;
                    newGene.enabled = (p1gene.enabled && p2gene.enabled) ? true : !(rand.NextDouble() < disabledIfEitherParentDisabledRate);
                    if (rand.NextDouble() < matingWeightAverageRate)
                        newGene.weight = (p1gene.weight + p2gene.weight) / 2;
                    else
                        newGene.weight = rand.NextDouble() < 0.5 ? p1gene.weight : p2gene.weight;
                    child.connectionGenes.Add(newGene);
                }
            }

            //Other genes
            if (o1.fitness > o2.fitness)
            {
                child.connectionGenes.AddRange(parent1.connectionGenes.Skip(numSharedGenes));
                child.numHiddenNodes = parent1.numHiddenNodes;
            }
            else if (o2.fitness > o1.fitness)
            {
                child.connectionGenes.AddRange(parent2.connectionGenes.Skip(numSharedGenes));
                child.numHiddenNodes = parent2.numHiddenNodes;
            }
            else //Equal, so add randomly
            {
                Queue<ConnectionGene> p1g = new Queue<ConnectionGene>(parent1.connectionGenes.Skip(numSharedGenes));
                Queue<ConnectionGene> p2g = new Queue<ConnectionGene>(parent2.connectionGenes.Skip(numSharedGenes));
                while (p1g.Count > 0 && p2g.Count > 0)
                {
                    ConnectionGene newGene;
                    if (p1g.Count == 0)
                        newGene = p2g.Dequeue();
                    else if (p2g.Count == 0)
                        newGene = p1g.Dequeue();
                    else
                    {
                        newGene = p1g.Peek().innovationNumber > p2g.Peek().innovationNumber ?
                                        p2g.Dequeue() :
                                        p1g.Dequeue();
                    }
                    if (rand.NextDouble() < 0.5)
                        child.connectionGenes.Add(newGene);
                }
                child.numHiddenNodes = Math.Max(parent1.numHiddenNodes, parent2.numHiddenNodes);
            }

            return child;
        }

        private void Mutate(Genome parentGenome)
        {
            if (rand.NextDouble() < mutateAddNewConnectionRate)
            {
                MutateAddConnection(parentGenome);
            }
            else if (rand.NextDouble() < mutateAddNewNodeRate)
            {
                MutateAddNode(parentGenome);
            }
            else if (rand.NextDouble() < mutateWeightsRate)//Do a normal weight mutation
            {
                for (int g = 0; g < parentGenome.connectionGenes.Count; g++)
                {
                    ConnectionGene gene = parentGenome.connectionGenes[g];
                    if (rand.NextDouble() < mutateUniformPerturbWeightRate)
                    {
                        gene.weight *= (rand.NextDouble() + .5);
                        if (rand.NextDouble() < mutateWeightFlipSignRate) gene.weight *= -1;

                    }
                    else
                    {
                        gene.weight = randNorm.Sample();
                    }
                }
            }
        }

        private void FormSpecies(List<Organism> offspring)
        {
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
                    if (Distance(species.representativeGenome, organism.genome) < compatabilityThreshhold)
                    {
                        species.members.Add(organism);
                        speciesFound = true;
                        break;
                    }
                }
                if (!speciesFound)
                {
                    Species newSpecies = new Species();
                    newSpecies.speciesNumber = nextSpeciesNumber++;
                    newSpecies.representativeGenome = organism.genome;
                    newSpecies.members = new List<Organism>() { organism };
                }
            }
            for (int i = 0; i < allSpecies.Count; i++)
            {
                if (allSpecies[i].members.Count == 0)
                {
                    allSpecies.RemoveAt(i);
                    i--;
                }
            }
        }

        //Calculate how many ticks a useless neural network will survive.
        long CalculateBaseTicksSurvived()
        {
            NeuralNetwork net = new NeuralNetwork(new Genome(numRows * numCols + 1, numOutputs, 0));
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
            return ticksSurvived;
        }

        public void NetworkStep(NeuralNetwork network, Board board)
        {
            double[] inputs = new double[numRows * numCols + 1];
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    int color = board.board[row + board.numHiddenRows, col];
                    inputs[row * numCols + col] = color == board.boardColor ? 0 : -1;
                }
            }
            Block block = board.currentBlock;
            for (int row = 0; row < block.squares.GetLength(0); row++)
            {
                for (int col = 0; col < block.squares.GetLength(1); col++)
                {
                    Coordinate coord = new Coordinate(row, col);
                    coord = block.toBoardCoordinates(coord);
                    if (block.squares[row, col] && coord.col >= 0 && coord.col < numCols
                            && coord.row >= board.numHiddenRows && coord.row < numRows + board.numHiddenRows)
                    {
                        inputs[(coord.row - board.numHiddenRows) * numCols + coord.col] = 1;
                    }
                }
            }
            inputs[inputs.Length - 1] = 1; //Bias input

            double[] outputs = network.FeedForward(inputs);
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
