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

        const double ACTIVATION_NOT_CALCULATED = Double.NaN;

        public NeuralNetwork(Genome genome)
        {
            numInputs = genome.numInputs;
            numOutputs = genome.numOutputs;
            NonInputNode[] unorderedNodes = new NonInputNode[genome.numOutputs + genome.numHiddenNodes];
            for(int i = 0; i < unorderedNodes.Length; i++)
                unorderedNodes[i] = new NonInputNode();

            //Create nodes
            foreach (ConnectionGene connection in genome.connectionGenes.Where(gene => gene.enabled))
            {
                int outNodeIndex = connection.outNodeNum - numInputs;
                NonInputNode outNode = unorderedNodes[outNodeIndex];

                outNode.sourceNodeNums.Add(connection.inNodeNum);
                outNode.sourceNodeWeights.Add(connection.weight);
                outNode.number = connection.outNodeNum;

            }

            //Order nodes using sort-of DFS
            List<NonInputNode> orderedNodeList = new List<NonInputNode>(unorderedNodes.Length);

            bool[] nodesCalculated = new bool[numInputs + unorderedNodes.Length];
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
        }

        public double[] FeedForward(double[] inputs)
        {
            //Activations are in the order (inputs, outputs, hidden nodes)
            double[] activations = Enumerable.Repeat(ACTIVATION_NOT_CALCULATED, numInputs + nonInputNodes.Length).ToArray();
            //Input node activations are just the inputs
            inputs.CopyTo(activations, 0);

            //NOTE: nodes are already in an order such that when we get to a node,
            //all of its source activations will have been calculated.
            for(int i = 0; i < nonInputNodes.Length; i++)
            {
                NonInputNode node = nonInputNodes[i];
                // Dot product
                activations[node.number] = ActivationFunc(
                                              Enumerable.Zip(node.sourceNodeNums, node.sourceNodeWeights,
                                                  (nodeIndex, weight) => activations[nodeIndex] * weight)
                                              .Sum()
                                           );
                if(Double.IsNaN(activations[node.number]))
                {
                    throw new InvalidOperationException("Nodes are not in a correct order.");
                }
            }
            //Take outputs from the end
            double[] outputs = activations.Skip(numInputs).Take(numOutputs).ToArray();
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
