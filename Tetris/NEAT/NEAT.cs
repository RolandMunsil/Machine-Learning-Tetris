using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

        readonly int populationSize = 1000;
        readonly int genZeroSpeciesSize = 5;
        readonly int minGenZeroConnections = 2;
        readonly int maxGenZeroConnections = 4;
        readonly double chanceOfGenZeroAddNode = .1;
        public readonly double c1 = 1;
        public readonly double c2 = 1;
        public readonly double c3 = 0.4;
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
            rand = new Random(0);
            randNorm = new MathNet.Numerics.Distributions.Normal(rand);
            connectionInnovations = new Dictionary<Tuple<int, int>, int>();
            addNodeInnovations = new Dictionary<int, int>();
            allSpecies = new List<Species>();
            baseTicksSurvived = CalculateBaseTicksSurvived();
        }

        public void MakeGenerationZero()
        {
            for (int i = 0; i < populationSize / genZeroSpeciesSize; i++)
            {
                Genome g = new Genome(numRows * numCols + 1, numOutputs, 0);

                //Add connections
                int numConnections = rand.Next(minGenZeroConnections, maxGenZeroConnections + 1);
                for (int c = 0; c < numConnections; c++)
                {
                    MutateAddConnection(g);
                    if (!g.GenesAreInOrder()) Debugger.Break();
                }

                //Add hidden node
                if(rand.NextDouble() < chanceOfGenZeroAddNode)
                {
                    MutateAddNode(g);
                    if (!g.GenesAreInOrder()) Debugger.Break();
                }

                if (!g.GenesAreInOrder()) Debugger.Break();

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
            genome.InsertGene(newGene1);
            genome.InsertGene(newGene2);
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
            genome.InsertGene(new ConnectionGene(inNode, outNode, randNorm.Sample(), true, innovationNum));
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
                if (numOffspring > 0 && species.members.Count > 5)
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
                    //Mutate or mate? (always mutate if only 1 member of species)
                    if (possibleParents.Count == 1 || rand.NextDouble() < mutationRate)
                    {
                        Genome parentGenome = possibleParents[rand.Next(possibleParents.Count)].genome.Clone();
                        Mutate(parentGenome);
                        if (parentGenome.connectionGenes.Count == 0) Debugger.Break();
                        if (!parentGenome.GenesAreInOrder()) Debugger.Break();
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
                        if (!child.GenesAreInOrder()) Debugger.Break();
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
            if (species.members.OrderByDescending(o => o.fitness).Last().fitness > species.members.OrderByDescending(o => o.fitness).First().fitness)
                throw new Exception();
            int numOrganismsToReturn = (int)Math.Ceiling(species.members.Count * survivalThreshhold);
            return species.members.OrderByDescending(o => o.fitness).Take(numOrganismsToReturn).ToList();
        }

        private Genome Mate(Organism o1, Organism o2)
        {
            Genome parent1 = o1.genome;
            Genome parent2 = o2.genome;

            Genome child = new Genome(parent1.numInputs, parent1.numOutputs, Int32.MinValue);
            child.numInputs = parent1.numInputs;
            child.numOutputs = parent1.numOutputs;

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
                    newGene.enabled = (gene1.enabled && gene2.enabled) ? true : !(rand.NextDouble() < disabledIfEitherParentDisabledRate);
                    if (rand.NextDouble() < matingWeightAverageRate)
                        newGene.weight = (gene1.weight + gene2.weight) / 2;
                    else
                        newGene.weight = rand.NextDouble() < 0.5 ? gene1.weight : gene2.weight;
                    child.connectionGenes.Add(newGene);

                    i1++;
                    i2++;
                }
            }
            foreach(ConnectionGene gene in child.connectionGenes)
            {
                child.numHiddenNodes = Math.Max(child.numHiddenNodes, Math.Max(gene.inNodeNum, gene.outNodeNum));
            }
            return child;
        }

        private void PossiblyAdd(Organism o1, Organism o2, Genome child, ConnectionGene gene1)
        {
            if (o1.fitness > o2.fitness)
            {
                if (!gene1.enabled)
                    gene1.enabled = !(rand.NextDouble() < disabledIfEitherParentDisabledRate);
                child.connectionGenes.Add(gene1);
            }
            else if (o1.fitness == o2.fitness)
            {
                if (!gene1.enabled)
                    gene1.enabled = !(rand.NextDouble() < disabledIfEitherParentDisabledRate);
                if (rand.NextDouble() < 0.5)
                    child.connectionGenes.Add(gene1);
            }
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
                    //Cuz its a struct.
                    parentGenome.connectionGenes[g] = gene;
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
                    allSpecies.Add(newSpecies);
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

            if (allSpecies.Exists(s => s.members.Count == 0))
                throw new Exception();
            if (allSpecies.Sum(s => s.members.Count) != populationSize)
                throw new Exception();
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
