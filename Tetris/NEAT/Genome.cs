using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    //Because NEAT focuses on connections, nodes themselves do not really have any information
    //associated with them other than if they are an input, output, or are hidden. So one way to organize the
    //nodes would be to have a list of Node structs, where the Node struct just consists of a single variable
    //signifying whether it's an input, output, or hidden. However, this wastes memory and time, as every time
    //a genome is created we would have to create and store ~205 Node structs.

    //The way I do it is that a "reference" to a node is just an integer, and each node has a unique integer.
    //The numberering of these node references is in the order (inputs, outputs, hidden nodes), which makes it
    //easy to tell if a given node is an input, output, or hidden.

    //Note that this means there really is no Node class or struct (in the Genome class at least) - we only use
    //references to nodes.

    //ALSO, it is important to understand that the way I implemented NEAT, I separated genomes from neural networks.
    //So the neural network class does actually have a Node sub-struct because it makes sense for actually doing
    //feedforward.

    public class Genome
    {
        public int numInputs;
        public int numOutputs;
        public HashSet<int> hiddenNodes;

        //Connection genes are always in order by innovation number. This is to make calculating Compatability easier and faster.
        public List<ConnectionGene> connectionGenes;

        public Genome(int numInputs, int numOutputs, IEnumerable<int> hiddenNodes)
        {
            this.numInputs = numInputs;
            this.numOutputs = numOutputs;
            this.hiddenNodes = new HashSet<int>(hiddenNodes);
            this.connectionGenes = new List<ConnectionGene>();
        }

        //A Genome with no hidden nodes
        public Genome(int numInputs, int numOutputs)
        {
            this.numInputs = numInputs;
            this.numOutputs = numOutputs;
            this.hiddenNodes = new HashSet<int>();
            this.connectionGenes = new List<ConnectionGene>();
        }

        public Genome Clone()
        {
            Genome copy = new Genome(numInputs, numOutputs, hiddenNodes);
            copy.connectionGenes.AddRange(connectionGenes);
            return copy;
        }

#if DEBUG
        public bool GenesAreInOrder()
        {
            return connectionGenes.SequenceEqual(connectionGenes.OrderBy(g => g.innovationNumber));
        }

        /// <summary>
        /// Check to make sure the HashMap of hidden nodes is consistent with the hidden nodes referenced by the connections
        /// in connectionGenes
        /// </summary>
        public void CheckConnectionsMatchHiddenNodes()
        {
            List<int> allReferencedNodes = connectionGenes.SelectMany(g => new[] { g.inNode, g.outNode }).Distinct().ToList();
            foreach(int node in allReferencedNodes)
            {
                if(node >= numInputs + numOutputs && !hiddenNodes.Contains(node))
                {
                    throw new Exception("A hidden node is referenced that is not in hiddenNodes!");
                }
            }
            foreach(int node in hiddenNodes)
            {
                if(!allReferencedNodes.Contains(node))
                {
                    throw new Exception("There is a node that has no references!");
                }
            }
        }
#endif

        public void AddGene(ConnectionGene gene)
        {
#if DEBUG
            if (NodeDependsOn(gene.inNode, gene.outNode))
                throw new Exception("Adding this gene would create a recursive loop.");
#endif
            //Add in node and out node to HashSet of hidden nodes
            if (IsHidden(gene.inNode))
                hiddenNodes.Add(gene.inNode);
            if (IsHidden(gene.outNode))
                hiddenNodes.Add(gene.outNode);

            //Place gene in in a position in the list that keeps it sorted.
            //Most common case is adding to the end, so we check that first.
            if (connectionGenes.Count == 0 ||
                gene.innovationNumber > connectionGenes[connectionGenes.Count - 1].innovationNumber)
            {
                connectionGenes.Add(gene);
            }
            else
            {
                int insertAfterIndex = connectionGenes.FindLastIndex(g => g.innovationNumber < gene.innovationNumber);
                connectionGenes.Insert(insertAfterIndex + 1, gene);
            }
#if DEBUG
            if (!GenesAreInOrder()) throw new Exception("Genes are not in order");
            CheckConnectionsMatchHiddenNodes();
            if (connectionGenes.GroupBy(g => g.innovationNumber).Count() != connectionGenes.Count)
                throw new Exception("There are duplicate gene innovation numbers");
            if (connectionGenes.GroupBy(g => new Tuple<int, int>(g.inNode, g.outNode)).Count() != connectionGenes.Count)
                throw new Exception("There are duplicate gene connections");
#endif
        }

        public bool IsInput(int node)
        {
            return node < numInputs;
        }
        public bool IsOutput(int node)
        {
            return !IsInput(node) && !IsHidden(node);
        }
        public bool IsHidden(int node)
        {
            return node >= (numInputs + numOutputs);
        }

        public IEnumerable<int> GetInputsTo(int node)
        {
            return from gene in connectionGenes
                   where gene.outNode == node
                   select gene.inNode;
        }
        public IEnumerable<int> GetOutputsOf(int node)
        {
            return from gene in connectionGenes
                   where gene.inNode == node
                   select gene.inNode;
        }

        public bool NodeDependsOn(int node, int possibleDependNode)
        {
            foreach (int inputTo in GetInputsTo(node))
            {
                if (inputTo == possibleDependNode || NodeDependsOn(inputTo, possibleDependNode))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates numbers that are needed to determine compatability.
        /// </summary>
        /// <param name="genome1"></param>
        /// <param name="genome2"></param>
        /// <param name="numDisjoint">The number of disjoint genes</param>
        /// <param name="numExcess">The number of excess genes</param>
        /// <param name="weightDiff">The total difference between weights of matching genes</param>
        /// <param name="numMatching">The number of genes present in both genomes</param>
        public static void CompatabilityParts(Genome genome1, Genome genome2, out int numDisjoint, out int numExcess, out double weightDiff, out int numMatching)
        {
            numDisjoint = 0;
            numExcess = 0;
            weightDiff = 0;
            numMatching = 0;
            int i1 = 0;
            int i2 = 0;
            while (true)
            {
                if (i1 == genome1.connectionGenes.Count)
                {
                    numExcess = genome2.connectionGenes.Count - i2;
                    break;
                }
                if (i2 == genome2.connectionGenes.Count)
                {
                    numExcess = genome1.connectionGenes.Count - i1;
                    break;
                }

                ConnectionGene gene1 = genome1.connectionGenes[i1];
                ConnectionGene gene2 = genome2.connectionGenes[i2];
                if (gene1.innovationNumber < gene2.innovationNumber)
                {
                    numDisjoint++;
                    i1++;
                }
                else if (gene2.innovationNumber < gene1.innovationNumber)
                {
                    numDisjoint++;
                    i2++;
                }
                else //equal
                {
                    numMatching++;
                    weightDiff += Math.Abs(gene1.weight - gene2.weight);
                    i1++;
                    i2++;
                }
            }
        }

        public override string ToString()
        {
            return $"{numInputs} in, {numOutputs} out, {hiddenNodes.Count} hidden nodes, {connectionGenes.Count(g => g.enabled)}/{connectionGenes.Count} enabled connections";
        }
    }
}
