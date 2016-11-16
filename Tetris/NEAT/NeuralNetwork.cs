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
            public List<int> sourceNodeNums;
            public List<double> sourceNodeWeights;
            public int number;

            public NonInputNode()
            {
                sourceNodeNums = new List<int>();
                sourceNodeWeights = new List<double>();
            }
        }

        public int numInputs;
        public int numOutputs;
        //The non-input nodes, NOT ordered by number! Ordered by calculation order.
        public NonInputNode[] nonInputNodes;
        public int totalNodes;

        public NeuralNetwork(Genome genome)
        {
            numInputs = genome.numInputs;
            numOutputs = genome.numOutputs;
            totalNodes = genome.numInputs + genome.numOutputs + genome.numHiddenNodes;
            List<NonInputNode> unorderedNodes = new List<NonInputNode>(genome.numOutputs + genome.numHiddenNodes);
            for (int i = 0; i < genome.numOutputs + genome.numHiddenNodes; i++)
            {
                unorderedNodes.Add(new NonInputNode());
                unorderedNodes[i].number = i + numInputs;
            }

            //Create nodes
            foreach (ConnectionGene connection in genome.connectionGenes.Where(gene => gene.enabled))
            {
                int outNodeIndex = connection.outNodeNum - numInputs;
                NonInputNode outNode = unorderedNodes[outNodeIndex];

                outNode.sourceNodeNums.Add(connection.inNodeNum);
                outNode.sourceNodeWeights.Add(connection.weight);
                if (outNode.number != connection.outNodeNum)
                    throw new Exception();
            }

            //Recursively remove any nodes that have no inputs
            for(int i = 0; i < unorderedNodes.Count; i++)
            {
                if(unorderedNodes[i].sourceNodeNums.Count == 0)
                {
                    int num = unorderedNodes[i].number;
                    unorderedNodes.RemoveAt(i);
                    //Remove references to node
                    foreach(NonInputNode node in unorderedNodes.Where(node => node.sourceNodeNums.Contains(num)))
                    {
                        int index = node.sourceNodeNums.IndexOf(num);
                        node.sourceNodeNums.RemoveAt(index);
                        node.sourceNodeWeights.RemoveAt(index);
                    }
                    //Restart search now that graph has been updated
                    i = -1;
                }
            }

            //Order nodes using sort-of DFS
            List<NonInputNode> orderedNodeList = new List<NonInputNode>(unorderedNodes.Count);

            bool[] nodesCalculated = new bool[totalNodes];
            for (int i = 0; i < numInputs; i++)
                nodesCalculated[i] = true;
            
            Queue<NonInputNode> nodeQueue = new Queue<NonInputNode>(unorderedNodes);
            while(nodeQueue.Count > 0)
            {
                NonInputNode candidateNode = nodeQueue.Dequeue();
                //If all of its sources will have been calculated
                bool canBeAdded = true;
                foreach(int num in candidateNode.sourceNodeNums)
                {
                    if(!nodesCalculated[num])
                    {
                        //Can't be put into the list yet
                        canBeAdded = false;
                        break;
                    }
                }
                if (canBeAdded)
                {
                    nodesCalculated[candidateNode.number] = true;
                    orderedNodeList.Add(candidateNode);
                }
                else
                    //Try it again once more nodes have been added
                    nodeQueue.Enqueue(candidateNode);
            }
            nonInputNodes = orderedNodeList.ToArray();

            if(nonInputNodes.Where(node => node.number > (numInputs + numOutputs - 1) && node.sourceNodeNums.Count == 0).Count() > 0)
            {
                throw new InvalidOperationException();
            }
        }

        public double[] FeedForward(double[] inputs)
        {
            //Activations are in the order (inputs, outputs, hidden nodes)
            double[] activations = Enumerable.Repeat(Double.NaN, totalNodes - numInputs).ToArray();
            //Input node activations are just the inputs
            //inputs.CopyTo(activations, 0);

            //NOTE: nodes are already in an order such that when we get to a node,
            //all of its source activations will have been calculated.
            for(int i = 0; i < nonInputNodes.Length; i++)
            {
                NonInputNode node = nonInputNodes[i];
                int activationIndex = node.number - numInputs;
                if (!Double.IsNaN(activations[activationIndex]))
                    throw new InvalidOperationException();

                // Dot product
                activations[activationIndex] = 0;
                for (int j = 0; j < node.sourceNodeNums.Count; j++)
                {
                    int sourceNode = node.sourceNodeNums[j];
                    double weight = node.sourceNodeWeights[j];
                    if (sourceNode < numInputs)
                        activations[activationIndex] += inputs[sourceNode] * weight;
                    else
                        activations[activationIndex] += activations[sourceNode - numInputs] * weight;
                }
                activations[activationIndex] = ActivationFunc(activations[activationIndex]);

                if(Double.IsNaN(activations[activationIndex]))
                {
                    throw new InvalidOperationException("Nodes are not in a correct order.");
                }
            }
            //Take outputs from the end
            double[] outputs = activations.Take(numOutputs).ToArray();
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
