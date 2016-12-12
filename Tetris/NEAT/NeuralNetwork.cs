using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Tetris.NEAT
{
    public class NeuralNetwork
    {
        [DebuggerDisplay("Node #{number}; inputs: {string.Join(\", \", sourceNodeNums.ToArray())}")]
        public class NonInputNode
        {
            public List<int> sourceNodes;
            public List<double> sourceNodeWeights;
            public int number;

            public NonInputNode(int number)
            {
                sourceNodes = new List<int>();
                sourceNodeWeights = new List<double>();
                this.number = number;
            }

            public void AddSource(int node, double weight)
            {
                sourceNodes.Add(node);
                sourceNodeWeights.Add(weight);
            }
        }

        public int numInputs;
        public int numOutputs;
        //The non-input nodes, ordered by calculation order.
        public NonInputNode[] nonInputNodes;

        public NeuralNetwork(Genome genome)
        {
            numInputs = genome.numInputs;
            numOutputs = genome.numOutputs;
            Dictionary<int, NonInputNode> nodesDict = new Dictionary<int, NonInputNode>();

            //Add the nodes
            foreach (int hiddenNode in genome.hiddenNodes)
                nodesDict.Add(hiddenNode, new NonInputNode(hiddenNode));
            for (int i = numInputs; i < (numInputs + numOutputs); i++)
                nodesDict.Add(i, new NonInputNode(i));

            //Add sources to nodes.
            foreach (ConnectionGene connection in genome.connectionGenes)
            {
                if (connection.enabled)
                {
                    NonInputNode outNode = nodesDict[connection.outNode];
                    outNode.AddSource(connection.inNode, connection.weight);
                }
            }
            List<NonInputNode> unorderedNodes = new List<NonInputNode>(nodesDict.Values);

            //Remove any nodes that have no inputs
            for (int i = 0; i < unorderedNodes.Count; i++)
            {
                if (unorderedNodes[i].sourceNodes.Count == 0)
                {
                    int num = unorderedNodes[i].number;
                    unorderedNodes.RemoveAt(i);
                    //Remove references to node
                    foreach (NonInputNode node in unorderedNodes.Where(node => node.sourceNodes.Contains(num)))
                    {
                        int index = node.sourceNodes.IndexOf(num);
                        node.sourceNodes.RemoveAt(index);
                        node.sourceNodeWeights.RemoveAt(index);
                    }
                    //Restart search now that graph has been updated
                    i = -1;
                }
            }

            //Order nodes using sort-of DFS
            List<NonInputNode> orderedNodeList = new List<NonInputNode>(unorderedNodes.Count);
            HashSet<int> nodesCalculated = new HashSet<int>();
            
            Queue<NonInputNode> nodeQueue = new Queue<NonInputNode>(unorderedNodes);
            while(nodeQueue.Count > 0)
            {
                NonInputNode candidateNode = nodeQueue.Dequeue();

                bool canBeAdded = true;
                foreach(int node in candidateNode.sourceNodes)
                {
                    if (node < numInputs)
                        continue;
                    if(!nodesCalculated.Contains(node))
                    {
                        //Can't be put into the list yet
                        canBeAdded = false;
                        break;
                    }
                }
                if (canBeAdded)
                {
                    nodesCalculated.Add(candidateNode.number);
                    orderedNodeList.Add(candidateNode);
                }
                else
                    //Try it again once more nodes have been added
                    nodeQueue.Enqueue(candidateNode);
            }
            nonInputNodes = orderedNodeList.ToArray();
        }

        public double[] FeedForward(double[] inputs)
        {
            //Originally I used an array to store activations but the nodes are so sparse that it's
            //quicker to use a dictionary.
            Dictionary<int, double> activations = new Dictionary<int, double>(nonInputNodes.Length);

            //Nodes are already in an order such that when we get to a node, all of its source activations 
            //will have been calculated, so we just calculate them in order.
            for (int i = 0; i < nonInputNodes.Length; i++)
            {
                NonInputNode node = nonInputNodes[i];

                // Dot product
                double dotProduct = 0;
                for (int j = 0; j < node.sourceNodes.Count; j++)
                {
                    int sourceNode = node.sourceNodes[j];
                    double weight = node.sourceNodeWeights[j];
                    if (sourceNode < numInputs)
                        dotProduct += inputs[sourceNode] * weight;
                    else
                        dotProduct += activations[sourceNode] * weight;
                }
                activations[node.number] = ActivationFunc(dotProduct);
            }
            //Take outputs
            double[] outputs = new double[numOutputs];
            for (int i = 0; i < numOutputs; i++)
            {
                //TryGetValue stores a 0 if the key isn't present, which works fine.
                activations.TryGetValue(numInputs + i, out outputs[i]);
            }
            return outputs;
        }

        public double ActivationFunc(double x)
        {
            //A modified version of the sigmoidal transfer function, used by
            //Stanley & Miikkulainen in their paper.
            return 1 / (1 + Math.Exp(x * -4.9));
        }
    }
}
